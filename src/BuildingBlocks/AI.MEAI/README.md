# VK.Blocks.AI.MEAI

`VK.Blocks.AI.MEAI` は、`Microsoft.Extensions.AI` (MEAI) をベースに、純粋な Ports 定義（`VK.Blocks.AI`）に対するマルチプロバイダー実装（OpenAI、Azure OpenAI、Ollama、Google Gemini、および OpenAI 互換プロバイダー）と、決定論的オニオン型ガバナンスパイプラインを提供するインフラストラクチャアダプターです。

関連仕様: [AI Ownership Map](/docs/03-Architecture/AI/ownership-map.md) / [Module Manifest](/src/BuildingBlocks/AI.MEAI/.prompts/module-manifest.md)

## 主な責務と設計原則

1. **Adapter-Only 境界 (`ARCH-02`, `ADR-020`, Zero-SDK-Leakage)**:
   - `Microsoft.Extensions.AI` および各ベンダー SDK の具象型（`IChatClient`, `IEmbeddingGenerator<string, Embedding<float>>` 等）はパッケージ内部にカプセル化され、外部への公開面は純粋な `VK.Blocks.AI` Ports（`IVKChatEngine`, `IVKTextEngine`, `IVKSpeechEngine`, `IVKTranscriptionEngine`, `IVKImageGenerationEngine`, `IVKEmbeddingsEngine`）と型付き Options として提供されます。
2. **決定論的オニオン・ガバナンスパイプライン (`ARCH-12`, `MEA-03`)**:
   - リクエストの安全性・観測性・決定論性を保証するため、チャットクライアントのパイプライン順序は外周から内周へ向けて厳密に固定されています（各層の有効/無効のみ設定可能で、並び替えは不可）：
     1. **OpenTelemetry & Audit**（最外周）:
        - `UseOpenTelemetry`: 生プロンプト・出力を除外（`EnableSensitiveData = false`）した分散トレース・メトリクス計測 (`OR.01`, `MEA-08`)
        - `AuditChatClient`: 実行時間・モデル ID・トークン消費量の構造化ログ記録 (`OR.01`, `FS-11`)
     2. **Idempotency & Caching**:
        - `IdempotencyChatClient`: 論理モデル呼び出し単位の冪等性と安全なキャッシュ復元 (`OWN-CALL-IDEMPOTENCY`, `FS-06`, `MEA-05`)。キャッシュヒット時は内側のツール実行およびルーティングをバイパス
     3. **Function Invocation**:
        - `FunctionInvocationGateChatClient` + `UseFunctionInvocation`: 単一モデル呼び出し内のツール実行ループ (`OWN-TOOL-LOOP`)。上流ランタイム（例: `AI.AgentFramework`）が `vk.autoInvokeTools = false` を指定した場合は二重実行を防止してバイパス
     4. **OutboundGuard & RequestCap**:
        - `PromptInjectionChatClient`: 組み立て済み送信リクエストに対するプロンプトインジェクション検出・遮断 (`OWN-OUTBOUND-GUARD`)
        - `PrivacyChatClient`: 個人識別情報 (PII) の自動マスキング（非テキスト `AIContent` は保持）
        - `TokenGovernanceChatClient`: レート制限 (`IVKTokenRateLimiter`)、プロバイダー応答 `UsageDetails` に基づく実使用量集計 (`OWN-USAGE-ACTUAL`, `MEA-07`)、および単一モデルリクエストのハード上限強制 (`OWN-REQUEST-CAP`, `MEA-04`: `MaxContextTokens` 超過時は型付きエラーを送出し、**サイレントなコンテキスト裁断・履歴切詰めは行いません**)
     5. **Routing / Failover**（任意・`IVKAIRoutingPolicy` / `IVKAIInvocationObserver` 登録時に接続）:
        - `RoutingChatClient`: `AI.Synapse` 等のルーティング決定 (`OWN-ROUTE`) に従い、候補接続間の境界付きフェイルオーバー実行ループ (`OWN-FAILOVER-EXECUTION`, `MEA-03`) を担います（試行上限を遵守し、ストリーミング出力コミット後の切替を禁止）。確定した実使用量 (`OWN-USAGE-ACTUAL`) や失敗種別・所要時間は `IVKAIInvocationObserver` 経由で `AI.Synapse` へ報告します (`MEA-07`)
     6. **Per-Connection Transport**（最内周）:
        - プロバイダーアダプターが構築するベース `IChatClient`
3. **トランスポート耐障害性と責務分離 (`OR.03`, `OWN-TRANSPORT-RESILIENCE`, `MEA-06`)**:
   - 全プロバイダー向けの送信 HTTP トラフィックは、`VK.Blocks.Resilience`（`IVKPolicyRegistry`）によるプロバイダー別名前付きトランスポートパイプライン（指数バックオフ + ジッタ付きリトライ、試行ごとのタイムアウト）を経由し、SDK 内蔵のリトライは無効化されます。
   - サーキットブレーカー状態および接続クールダウンは保持せず、`AI.Synapse`（`OWN-CONN-HEALTH`）に完全に委譲します。

## 利用方法

```csharp
// DI 登録 (VKAIMEAIBlock)
services.AddVKAIMEAIBlock(configuration, options => options with
{
    DefaultProvider = "openai",
    ProviderSettings = new Dictionary<string, VKAIMEAIProviderSettings>(StringComparer.OrdinalIgnoreCase)
    {
        ["openai"] = new VKAIMEAIProviderSettings
        {
            Provider = VKAIProviderType.OpenAI,
            ChatModelId = "gpt-4o-mini",
            ApiKey = new VKSensitiveString("sk-...")
        }
    },
    ChainSettings = new VKAIMEAIClientChainSettings
    {
        EnableOpenTelemetry = true,
        EnableAudit = true,
        EnableInjectionGuard = true,
        EnablePrivacy = true,
        EnableTokenGovernance = true,
        MaxContextTokens = 8192,
        EnableCaching = false,
        EnableFunctionInvocation = true
    }
})
.AddOpenAI(); // または .AddAllProviders()
```
