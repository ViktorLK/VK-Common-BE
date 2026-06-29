# ADR 013: Multi Provider Resilience Failover via Composite Chat Completion Service

- **Date**: 2026-06-07
- **Status**: ✅ Accepted
- **Deciders**: Architecture Team
- **Technical Story**: src/BuildingBlocks/AI.SemanticKernel

## 1. Context (背景)

エンタープライズ製品において、大言語モデル (LLM) API の呼び出し（Chat Completion）はネットワークの中断や、一時的な API レート制限（HTTP 429 Too Many Requests）、またはプロバイダー自体のダウンタイムなどにより頻繁に失敗する。これらの障害から迅速に復旧し、エンドユーザーへのサービスを無停止で提供するためには、プライマリサービスが失敗した際に、自動的に代替のプロバイダーやバックアップモデル（例: OpenAI がダウンした際に Azure OpenAI または Claude に自動切り替え）に処理をフォールバックさせる「マルチエンジン・レジリエンス・フェイルオーバー」のメカニズムが必要である。

## 2. Problem Statement (問題定義)

アプリケーションコード側で手動でフェイルオーバーを実装したり、各エンジンに直接リトライ・代替エンジン登録を記述することには、以下の問題がある：
1. **ビジネスロジックの汚染**: 各 `IVKChatEngine` 内部に、例外をキャッチして別のクライアントインスタンスを呼び出す複雑な try/catch 制御コードが散乱し、可読性と保守性が低下する。
2. **Polly 統合の非効率**: 非同期ストリーミング（IAsyncEnumerable / StreamingChatMessageContent）の実行中において、最初の接続確立時や途中のストリーム障害時に動的にバックアップサービスへの切り替えと設定パラメータ（`ModelId` 等）の書き換えを行うのが困難である。
3. **DI 登録の複雑化**: 複数の `IChatCompletionService` を Semantic Kernel に登録する際、プライマリとバックアップを識別して実行時に透過的に使い分けるクリーンな仕組みが存在しない。

## 3. Decision (決定事項)

透過的かつ堅牢なフェイルオーバーを実現するため、**「Composite Chat Completion Service (複合チャット補完サービス)」**を採用する。

1. **`CompositeChatCompletionService` の開発**:
   - `Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService` 抽象インターフェースを直接実装する複合クラスを内部に構築する。
   - DI コンテナからプライマリ（`primary`）およびフォールバック（`fallback_0`、`fallback_1` 等）の名前付き（Named）サービスを解決できるように設計する。
2. **Polly v8 Resilience Pipeline による状態管理とリトライ**:
   - Polly v8 の `ResiliencePipelineBuilder` を用いて、リトライポリシーを構築する。
   - 対象とする例外（HTTP 429、一時的なネットワーク障害、サーキットブレーカー発動など）を検出した際、リトライ試行回数（`AttemptIndex`）をコンテキストにセットする。
   - リトライ実行時に、インデックスに応じたバックアップサービスを呼び出し、動的に `executionSettings.ModelId` を代替のモデルIDに書き換えて実行する。
3. **ストリーミング完了のサポート**:
   - 標準のブロッキング応答（`GetChatMessageContentsAsync`）とストリーミング応答（`GetStreamingChatMessageContentsAsync`）の双方に対して、Polly パイプラインを介した再接続と安全なフォールバックを透過的に実行する。

### 核心的な複合サービスとフェイルオーバーパイプライン設計

```csharp
namespace VK.Blocks.AI.SemanticKernel.Chat.Internal;

internal sealed class CompositeChatCompletionService : IChatCompletionService
{
    private readonly Microsoft.SemanticKernel.Kernel _kernel;
    private readonly IReadOnlyList<VKChatFallbackConfig> _fallbacks;
    private readonly ResiliencePipeline<IReadOnlyList<ChatMessageContent>> _pipeline;

    public CompositeChatCompletionService(
        Microsoft.SemanticKernel.Kernel kernel,
        IOptions<VKChatOptions> chatOptions,
        ILogger<CompositeChatCompletionService> logger)
    {
        _kernel = VKGuard.NotNull(kernel);
        _fallbacks = chatOptions?.Value?.ChatFallbacks ?? [];
        _pipeline = BuildPipeline();
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Microsoft.SemanticKernel.Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var context = ResilienceContextPool.Shared.Get(cancellationToken);
        context.Properties.Set(new ResiliencePropertyKey<PromptExecutionSettings?>("Settings"), executionSettings);

        try
        {
            return await _pipeline.ExecuteAsync(async (ctx) =>
            {
                int attemptIndex = 0;
                ctx.Properties.TryGetValue(new ResiliencePropertyKey<int>("AttemptIndex"), out attemptIndex);
                
                // インデックス 0 はプライマリ、1以上はフォールバックサービスを選択
                string serviceId = attemptIndex == 0 ? "primary" : $"fallback_{attemptIndex - 1}";
                var service = _kernel.GetRequiredService<IChatCompletionService>(serviceId);
                
                var currentSettings = ctx.Properties.GetValue(new ResiliencePropertyKey<PromptExecutionSettings?>("Settings"), null);
                
                // フォールバック試行時は、設定された代替モデルIDに動的書き換え
                if (attemptIndex > 0 && currentSettings != null)
                {
                    currentSettings.ModelId = _fallbacks[attemptIndex - 1].ModelId;
                }

                return await service.GetChatMessageContentsAsync(chatHistory, currentSettings, kernel ?? _kernel, ctx.CancellationToken).ConfigureAwait(false);
            }, context).ConfigureAwait(false);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(context);
        }
    }
}
```

## 4. Alternatives Considered (代替案の検討)

### Option 1: Handle Fallbacks in AISKChatEngine directly
- **Approach**: `AISKChatEngine` の呼び出し部分に try/catch を書き、エラー発生時に異なる `Kernel` 又は `IChatCompletionService` を手動ループで呼び出す。
- **Rejected Reason**: ストリーミング（`SendStreamingAsync`）呼び出し時に、途中で切断された際の再接続と部分消費済みストリームの制御が不可能に近く、かつコードが極めて複雑化するため。

### Option 2: API Gateway / Load Balancer Level Failover
- **Approach**: クラウドの API Gateway（例: Azure API Management や独自プロキシ）を噛ませて、そこで 429 時のフェイルオーバーを行う。
- **Rejected Reason**: ローカル開発環境での検証が困難になるほか、異なるプロバイダー間（例: Azure OpenAI から Anthropic への切り替え）での要求・応答のペイロード・プロトコル変換をプロキシ層で行うのが極めて困難であるため。

## 5. Consequences & Mitigation (結果と緩和策)

### Positive
- **完全な透過性**: `AISKChatEngine` を呼び出すアプリケーション層や他の BuildingBlock は、フェイルオーバーの存在を一切意識せず、ただ一つの `IVKChatEngine` を呼ぶだけで完璧な高可用性を得られる。
- **きめ細やかな設定**: 各 `VKChatFallbackConfig` に、代替先の `ModelId` だけでなく対応する資格情報やエンドポイント情報を明示的にマッピングできる。

### Negative
- **Polly 実行コンテキストの管理コスト**: スレッドプールから `ResilienceContext` を毎回取得・返却する（`ResilienceContextPool`）ため、微小なオブジェクト管理オーバーヘッドが発生する。

### Mitigation
- .NET 8 / Polly v8 の高性能プール設計（`ResilienceContextPool.Shared`）を採用することで、ヒープ割当（Allocation）をほぼゼロに抑え、パフォーマンス影響を完全に排除する。

## 6. Implementation & Security (実装詳細とセキュリティ考察)

- **Transient Error Classification**: 不要なリトライ（例: ユーザーの認証エラー（401）やリクエストフォーマットエラー（400））による API 課金と遅延を防ぐため、`HttpOperationException` のステータスコードをチェックし、`429`（Too Many Requests）および 5xx 系のシステム障害、ネットワークタイムアウトのみをリトライ対象に厳密に分類する。

## 7. Status
✅ Accepted
