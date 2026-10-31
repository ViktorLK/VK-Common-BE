# ADR 012: Dynamic Regex Pattern Matching Stage in Prompt Assembly

- **Date**: 2026-06-10
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.Psyche

## 1. Context (背景)

AI.Psyche において、プロンプトに動的なテキスト置換や挙動の変更を加える際、キーワードの一致だけでなく、複雑なパターン（電話番号の形式、特定の記法、ユーザー定義の動的な文字列など）を検知して置換を行ったり、特定のルールを発火させたいという要求がある。これらをあらかじめ静的な変数置換タスクだけで賄うことは難しく、またアプリケーションコードに正規表現と置換処理をハードコードすることは、パターンの動的な変更や保守の観点から望ましくない。

## 2. Problem Statement (問題定義)

静的な変数置換のみに依存する、またはマッチングルールをハードコードすることには、以下の課題がある：
1. **ルールの柔軟性欠如**: 管理画面やデータベースから動的に追加されたマッチングパターン（例: 特定の不適切ワードの置換ルール、追加のドキュメントタグ）に対して、コードを再コンパイル・再ビルドしないと追従できない。
2. **正規表現処理の重複**: 各開発者がそれぞれのハンドラーで独自に `Regex.Replace` を書き散らすことで、正規表現エンジンのコンパイルオプション（`Compiled` やタイムアウト時間）の設定にばらつきが生じ、パフォーマンス劣化や ReDoS 脆弱性が再発する。

## 3. Decision (決定事項)

正規表現パターンに基づく柔軟なテキスト変形・置換処理を安全に一元管理するため、AI.Psyche 内に **`Pattern` 特性（Feature）の導入**を決定する。

1. **`Pattern` 特性の新設**:
   - 識別する正規表現、置換後のテキスト、有効状態を定義する `VKPatternEntry`、およびそれらを読み書きする `IVKPatternStore` を定義する。
2. **`DefaultPatternStage` の実装**:
   - `IVKPsycheBeforePipelineStage` を継承した `DefaultPatternStage` を構築する。
   - Weaving が行われる前段階において、アクティブなパターンエントリー群を取得し、それらを順次ユーザー入力（`UserInput`）や組み立て中のフラグメントに対して適用し、安全に正規表現置換（`Regex.Replace`）を実行する。
3. **ReDoS 安全防線の適用**:
   - すべてのパターン置換処理において、正規表現オブジェクトのインスタンス化時に `TimeSpan.FromMilliseconds(100)` の実行タイムアウトを強制し、システムダウンを物理的に防止する。

### 核心的なパターンステージとデータ構造設計

```csharp
namespace VK.Blocks.AI.Psyche.Pattern.Models;

public sealed record VKPatternEntry
{
    public required VKPatternId Id { get; init; }
    
    // トリガーとなる正規表現パターン (e.g. @"\b(csharp|dotnet)\b")
    public required string RegexPattern { get; init; }
    
    // 置換後のテキスト
    public required string Replacement { get; init; }
    public bool IgnoreCase { get; init; } = true;
    public bool Enabled { get; init; } = true;
}

namespace VK.Blocks.AI.Psyche.Pattern.Internal;

internal sealed class DefaultPatternStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKPatternStore _store;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken ct)
    {
        var patternsResult = await _store.GetPatternsAsync(context.Request.TenantId, ct).ConfigureAwait(false);
        if (patternsResult.IsFailure) return VKResult.Failure(patternsResult.Errors);

        var userInput = context.Request.UserInput;
        
        // 登録されたアクティブな正規表現パターンを順次適用して入力を置換・補正
        foreach (var pattern in patternsResult.Value.Where(p => p.Enabled))
        {
            var options = pattern.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            try
            {
                // 100ms タイムアウトを強制した安全な Regex 呼出
                userInput = Regex.Replace(
                    userInput, 
                    pattern.RegexPattern, 
                    pattern.Replacement, 
                    options, 
                    TimeSpan.FromMilliseconds(100));
            }
            catch (RegexMatchTimeoutException)
            {
                // タイムアウト時はログを出力してスキップ (可用性優先)
            }
        }

        context.SetUserInput(userInput); // 置換後のクリーンな入力をContextに再格納
        return VKResult.Success();
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Integrate Pattern stage into Knowledge stage
- **Approach**: `Knowledge` 段階のクラス内で、知識取得とパターン置換を一緒に処理する。
- **Rejected Reason**: 知識（提示文の追加）とパターン（テキストの変形・置換）は、ドメインとしての関心が全く異なる（前者は情報の拡張、後者はフォーマットとサニタイズ）。これらを混ぜるとクラスの凝集度が下がり、テストが複雑になるため。

### Option 2: Pre-evaluating Patterns in Front-End/BFF
- **Approach**: クライアントアプリ側やゲートウェイ（BFF）層で、送信前に正規表現置換をかけてから送信する。
- **Rejected Reason**: テナントごとのビジネスルールや機密情報に関わる置換定義がクライアント側に漏れ出すことになり、セキュリティ保護の観点から容認できないため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **動的なプロンプト変形能力**: 運用中に DB や設定ファイルを書き換えるだけで、ソースコードを変更することなく、入力値の正規化や特定キーワードの強調、フォーマットの差し替え等を柔軟に実施できる。
- **安全第一の実装**: 強制的な 100ms 制限により、ユーザー定義の複雑な正規表現パターンが万が一バックトラックを引き起こしても、サーバー全体が停止するリスクが完全に排除される。

### Negative
- **処理レイテンシの増加**: パターン数が多く、かつユーザー入力テキストが長文である場合、リクエストごとの Regex 評価回数が増え、CPU 負荷が上昇する。

### Mitigation
- パターン定義が更新されない限り、コンパイル済みの `Regex` インスタンスをキャッシュし、毎回の `new Regex(...)` のインスタンス化アロケーションを回避する高速キャッシュ機構をストア層と併せて導入する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Regex Validation on Save**: 新しい `VKPatternEntry` をストアに登録・保存する段階において、あらかじめ仮のテキストで `Regex` の検証を行い、文法的に破綻しているパターンは保存前にエラーとして弾くバリデーションを適用する。

## 7. Status
✅ Accepted
