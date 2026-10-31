# ADR 008: Sliding Token Aware Pruning

- **Date**: 2026-05-31
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

LLM（大規模言語モデル）には、入力できる最大コンテキストウィンドウ（Token limit）が存在する。チャット履歴（Echo）などのテキストは時間とともに無限に累積するため、どこかのタイミングで切り捨て（Pruning / Truncation）を行う必要がある。単純に古い順に会話を削除するだけでは、非常に長いメッセージが含まれていた場合に入力制限を超えてAPIエラーとなり、逆に短文ばかりの場合には過剰に履歴を削除してコンテキスト情報を失いすぎてしまう。

## 2. Problem Statement (問題定義)

静的なメッセージ数ベースの切り捨てや、境界制御の甘い切り捨てロジックには、以下の問題がある：
1. **APIの物理上限超過によるクラッシュ**: リクエストサイズがモデルの上限トークン（例：8kトークンや32kトークン）を突然超え、通信エラーとなってビジネスプロセスが中断する。
2. **情報の早期喪失**: 会話の中身が数文字の短文ばかりであっても、「直近10件のみ残す」といった静的制限があると、モデルに十分な文脈情報を渡すことができず、応答品質が低下する。
3. **境界条件エラー**: 履歴データが存在しない場合、あるいは全体の長さが上限に極めて近い場合の境界値判定にバグが生じやすく、例外が発生しやすい。

## 3. Decision (決定事項)

プロンプト全体のトークン予算を効率的に使い切り、大モデルの物理的な入力限界を確実に守るため、**「Sliding Token-Aware Pruning (トークン予算に基づくスライディング履歴修剪)」**アルゴリズムを採用する。

1. **二重予算制 (Dual-Budgeting) の導入**:
   - 会話履歴修剪において、最大履歴件数（`MaxTurns`）と、プロンプトに割り当て可能な最大トークン容量比率（`TokenBudgetRatio` 又は `MaxTokens`）の双方を評価する。
2. **トークン数の正確な事前計算**:
   - `IVKTokenCounter` を使用して、現在のコンテキストに存在する各 Fragment の正確なトークン数を計算する。
3. **時間軸に基づくスライディングエビクション**:
   - 全体トークン数が割り当てられた予算上限を超える場合、最も古い会話Fragmentから順にコンテキストから動的に取り除き、`VKWeavingContext` の `Evicted`（已修剪プール）に移動させる。
   - この修剪処理は、全体のToken数が予算内に収まるまでループで順次実行され、例外を発生させることなく自動的に最新の会話の重要部分を保護する。

### 核心的な修剪ロジックの実装例

```csharp
namespace VK.Blocks.AI.Psyche;

internal sealed class DefaultEchoStage : IVKWeavingStage
{
    private readonly IVKEchoStore _store;
    private readonly IVKTokenCounter _tokenCounter;

    public async Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken ct)
    {
        // 1. ストアから全履歴を取得
        var historyResult = await _store.GetHistoryAsync(context.TenantId, context.SessionId, ct).ConfigureAwait(false);
        var activeFragments = historyResult.Data.ToList();

        // 2. 段階的制限 (MaxTurns) の適用
        var limit = context.Echo?.MaxTurns ?? _options.MaxTurns;
        while (activeFragments.Count > limit)
        {
            var oldest = activeFragments[0];
            activeFragments.RemoveAt(0);
            context.AddEvicted(oldest); // 已退避リストへ退避
        }

        // 3. トークン予算に基づく動的修剪
        var maxBudget = ResolveTokenBudget(context);
        var currentTokens = _tokenCounter.CountTokens(activeFragments);

        while (currentTokens > maxBudget && activeFragments.Count > 0)
        {
            var oldest = activeFragments[0];
            activeFragments.RemoveAt(0);
            context.AddEvicted(oldest);

            // トークン総数を再評価
            currentTokens = _tokenCounter.CountTokens(activeFragments);
        }

        context.SetFragments(activeFragments);
        return VKResult.Success();
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Naive Substring Truncation
- **Approach**: トークン数を考慮せず、単純に全体の文字列長（`Length`）が一定数を超えたら古い部分を文字列置換などでバッサリ切り落とす。
- **Rejected Reason**: 日本語のようなマルチバイト言語や、プロンプトのフォーマットタグ（XMLやJSONタグ）が文字の途中で切断されてしまい、LLMがパースエラーやフォーマット崩れを起こす原因になるため。

### Option 2: Throw Exception when Budget Exceeded
- **Approach**: 予算を超えた場合にWeavingを失敗（`Result.Failure`）とし、アプリケーション側に例外やエラーを返す。
- **Rejected Reason**: 会話が長くなるとユーザーが二度とメッセージを送信できなくなり、サービスが恒久的に利用できなくなる致命的な可用性低下を招くため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **100%のエラー回避**: トークン制限超過によるLLM API側の実行時エラー（API Error 400）が物理的に発生しなくなる。
- **情報の最大活用**: 許容範囲の中で、可能な限り多くの文脈履歴をモデルに渡すことができるため、会話の継続性が最適化される。

### Negative
- **トークンカウントの計算オーバーヘッド**: 履歴の修剪判定のためにトークンカウンターを何度も呼び出すと、カウント処理の計算コスト（Tokenizerのエンコード処理）が膨らみ、Weaving全体の処理性能に影響する。

### Mitigation
- トークン計算処理にキャッシュを導入する、または各Fragmentが生成された時点でそのトークン数をあらかじめプロパティとして保持させておき、修剪判定時は単なる整数の減算のみで計算が完了する構造とする（アロケーションを伴う再カウントの最小化）。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Graceful Eviction Tracing**: 修剪（Evict）された会話履歴の内容は、監査ログやトレース情報に記録できるようにし、どの情報がLLMに送信されなかったのかを開発者が追跡できるように配慮する。

## 7. Status
✅ Accepted
