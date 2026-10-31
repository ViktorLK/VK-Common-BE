# ADR 009: Applying Cross Cutting Concerns via Semantic Kernel Filters

- **Date**: 2026-06-03
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.SemanticKernel

## 1. Context (背景)

VK.Blocks.AI 体系において、AI を呼び出す全てのリクエスト（ChatCompletion や TextGeneration などの Engine）やネイティブ関数の実行（Plugin Function Invocation）に対して、Rate Limiting（限流）、Token 監査（Audit）、PII 脱敏（Masking）、およびプロンプトインジェクション検知などの横切的（Cross-cutting）な安全・管理機能を均一に適用する必要がある。これらの処理を各 Engine クラスに個別に直接記述することは、コードの重複と保守性の劣化を招く。

## 2. Problem Statement (問題定義)

横切的な関心事を個別の Engine クラス（例: `AISKChatEngine`、`AISKTextEngine`）に手動で埋め込む手法には、以下の課題がある：
1. **コードの深刻な重複**: 各エンジンクラスにトークン計数やレート制限の待機ロジックを何度も書く必要があり、エンジン実装自体が肥大化する。
2. **監査漏れのリスク**: 将来的に新しいエンジンやプロバイダー（例: 新規の音声・画像エンジンなど）が追加された際、開発者がこれら共通処理の呼び出しを忘れると、監査や安全防線に穴が空いてしまう。
3. **拡張性とプラグイン実行の無視**: 大モデルがネイティブな C# プラグイン（Tool call）を実行する中間プロセスに介入できず、プラグインを介した情報漏洩や不正な関数呼び出しの監視を行えない。

## 3. Decision (決定事項)

共通のシステム防線とアセンブリロジックをエンジンコードから完全に分離するため、**「Semantic Kernel Native Filters (セマンティックカーネル・ネイティブフィルタ)」**をインターセプターとして採用する。

1. **SK フィルターインターフェースの実装**:
   - `IPromptRenderFilter`（プロンプト生成時に介入）および `IFunctionInvocationFilter`（プラグイン関数実行時に介入）を実装するフィルター群（`AISKTokenicsFilter`、`AISKPrivacyFilter`、`AISKInjectionFilter`）を導入する。
2. **Tokenics / Rate Limiting の自動適用**:
   - `AISKTokenicsFilter` は、プロンプトがレンダリングされた直後に `IVKTokenCounter` で想定トークン数を算出し、`IVKTokenRateLimiter` を呼び出してリアルタイムで制限をかける。また、関数の実行結果メタデータから実際に入力・出力されたトークン数を抽出し、レートリミッターへ返報・監査ログ出力を行う。
3. **DI コンテナによる自動アタッチ**:
   - `AISKBlockRegistration.cs` において、これらのフィルターを `TryAddEnumerable` 等を用いて DI 登録し、`Kernel` インスタンス生成時に自動でカーネルのパイプラインにバインドされるように設計する。

### 核心的なフィルター設計と登録例

```csharp
namespace VK.Blocks.AI.SemanticKernel.Filters.Internal;

internal sealed class AISKTokenicsFilter(
    IVKTokenRateLimiter? rateLimiter = null,
    IVKTokenUsageAggregator? aggregator = null,
    IVKTokenCounter? tokenizer = null) : IPromptRenderFilter, IFunctionInvocationFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        VKGuard.NotNull(context);
        VKGuard.NotNull(next);

        // プロンプトをレンダリングして文字列化
        await next(context).ConfigureAwait(false);

        var renderedPrompt = context.RenderedPrompt;
        if (!string.IsNullOrWhiteSpace(renderedPrompt) && rateLimiter != null)
        {
            int estimatedTokens = tokenizer?.CountTokens(renderedPrompt) ?? (renderedPrompt.Length / 4);
            var acquireResult = await rateLimiter.AcquireAsync(estimatedTokens, context.CancellationToken).ConfigureAwait(false);
            if (acquireResult.IsFailure)
            {
                throw new VKDomainException(acquireResult.FirstError.Code, acquireResult.FirstError.Description);
            }
        }
    }

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // 処理の事前監査ログの出力、next() の呼出、事後の実際トークン数の集計処理などを実装
        await next(context).ConfigureAwait(false);
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Onion Middleware Pattern on IVKChatEngine
- **Approach**: `IVKChatEngine` の呼び出しをデリゲートチェーンでラップするカスタムミドルウェアを自作する。
- **Rejected Reason**: チャット呼び出しには割り込めるが、Semantic Kernel が内部で自律的にプラグイン関数（Tool call）をループ実行する過程（ファンクションコーリングループ）の細部には介入できず、プラグインの入力/出力の脱敏や監査が不完全になるため。

### Option 2: Core Decorator Pattern
- **Approach**: 各エンジンクラス（`AISKChatEngine` 等）を `TokenicsChatEngineDecorator` で装飾（Decorate）する。
- **Rejected Reason**: Engine クラスごと（Chat 用、Embedding 用、音声用など）に専用のデコレータを用意する必要があり、ボイラープレートコードが大量に発生するため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **完全な一元化**: 開発者は個々のエンジン実装内でレート制限や PII 脱敏を一切意識する必要がない。
- **プラグイン実行の安全性確保**: 大モデルがプラグインを実行するたびに介入できるため、パラメータのサニタイズや監査が漏れなく実施される。

### Negative
- **例外のリーク制御**: フィルター内でレート制限オーバー等の例外（`VKDomainException`）をスローした際、SK 内部でラップされ、上位の Engine クラスで元のエラーコードを正確にマッピングする処理が必要になる。

### Mitigation
- `AISKErrorMapper` にフィルター由来のドメイン例外（`VKDomainException`）をアンラップして、元の `VKResult.Failure` に正しく詰め替えるハンドリングを整備する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **PII Leakage Prevention**: `AISKPrivacyFilter` は、プロンプトの送信前に正規表現（`RegexPrivacyFilter`）を用いて個人情報（電話番号、クレカ情報など）を `[MASKED]` 等に置換し、外部の LLM プロバイダーへ漏洩するのを防御する。
- **Filter Registration Order**: フィルターの実行順序がセキュリティに与える影響を考慮し、「脱敏・安全検知（Privacy/Injection）」を最も外側に配置し、「レート制限（Tokenics）」はクリーンな文字列が生成された後に実行するように DI 登録順序を明示的に制御する。

## 7. Status
✅ Accepted
