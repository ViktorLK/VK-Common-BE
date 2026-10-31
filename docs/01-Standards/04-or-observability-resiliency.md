# 04. Observability & Resiliency (`OR.01`〜`OR.03`)

> **対応ルール**: [`.agents/rules/l3-vk-be-or-observability-resiliency.md`](/.agents/rules/l3-vk-be-or-observability-resiliency.md) (`OR.01`〜`OR.03`)

本ドキュメントは、ゼロアロケーション構造化ロギング（`[LoggerMessage]`）、OpenTelemetry メトリクス・分散トレーシング、テナント分離と PII ログマスキング、および Polly による外部通信レジリエンスの規約と実装例を定義します。

---

## `OR.01` — Observability (Logging, Metrics & Tracing) 🔴

### 1. High-Performance Structured Logging (`[LoggerMessage]`)
- **Source Generator Mandatory**: 本番コードで `logger.LogInformation(...)` や `logger.LogError(...)` などの標準メソッドを直接呼び出すことは禁止です。必ず `[LoggerMessage]` 属性を用いた `internal static partial class` の拡張メソッドとして定義します。
- **Single File Consolidation**: 1つの機能スライス内のすべての `[LoggerMessage]` メソッドは、`{FeatureName}/Diagnostics/Internal/{FeatureName}Diagnostics.cs` の1ファイルに集約します（別個の `XxxLog.cs` などを乱立させることは禁止です）。モジュール全体で共有するロガーのみ `Common/Diagnostics/Internal/` に配置します。
- **Structured Templates**: 文字列補間（`$"Key {id} failed"`）は厳禁です。必ず構造化テンプレート（`"{KeyId}"`, `"{TenantId}"`）を使用します。
- **TraceId & Exception Context**: すべてのログエントリとエラー応答には `TraceId` を含め、インフラ例外を `Result.Failure` にマッピングする際はその直前に必ず例外オブジェクト付きでログ記録します。

```csharp
namespace VK.Blocks.Authentication.ApiKeys.Diagnostics.Internal;

// [SG Diagnostics] - This attribute triggers the Source Generator to generate ActivitySource and Meter.
// [SG Logger] - This class is automatically implemented by the Source Generator for high-performance logging.
[VKBlockDiagnostics(typeof(VKAuthenticationBlock))]
internal static partial class ApiKeysDiagnostics
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "API Key authentication failed for KeyId: {KeyId}, TenantId: {TenantId}.")]
    internal static partial void LogApiKeyAuthFailed(
        this ILogger logger,
        string keyId,
        string? tenantId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "Failed to query API Key store for KeyId: {KeyId}.")]
    internal static partial void LogStoreQueryException(
        this ILogger logger,
        Exception ex,
        string keyId);
}
```

### 2. Metrics & Distributed Tracing
- **`[VKBlockDiagnostics]`**: 自動生成された `ActivitySource` と `Meter` を使用します。
- **Semantic Constants**: メトリクス名やタグキーはすべて `VK{ModuleName}DiagnosticsConstants.cs`（または `VK{FeatureName}DiagnosticsConstants.cs`）に OpenTelemetry Semantic Conventions に準拠した定数として定義します。
- **Instrument Selection**:
  - カウントには `Counter<T>`、所要時間・レイテンシには `Histogram<T>`、増減するゲージ値には `UpDownCounter<T>` を使用します。
  - **ループ内での Instrument 生成禁止**: `Meter.CreateCounter<long>(...)` 等は静的フィールドまたはシングルトン初期化時に1回だけ呼び出し、リクエストやループのたびに生成してはなりません。

```csharp
namespace VK.Blocks.Authentication;

public static class VKApiKeysDiagnosticsConstants
{
    public const string AuthAttemptsMetric = "vk.auth.apikey.attempts";
    public const string AuthDurationMetric = "vk.auth.apikey.duration";
    public const string TagTenantId = "tenant.id";
    public const string TagOutcome = "auth.outcome";
}
```

---

## `OR.02` — Security (Tenant Isolation & PII Masking)
- **EF Core Global Query Filter**: マルチテナント環境における `TenantId` フィルタリングは、必ず EF Core の Global Query Filter によって強制します。アプリケーションクエリでテナント分離をバイパス（`IgnoreQueryFilters()` の不正使用等）することは禁止です。
- **PII & Secret Masking**: 個人情報（メールアドレス、氏名、IP 等）および機密情報（生トークン、API キー平文、パスワード、接続文字列）は、`SensitiveDataProcessor` 等の専用マスキング機構を通じてログ出力前に必ずマスクします。

---

## `OR.03` — Resiliency (Polly)
外部システム（HTTP API、Azure SDK、サードパーティサービス）へのすべての呼び出しは、アダプター境界でレジリエンスポリシーによって保護します。

- **Minimum Policy**:
  1. **Retry (3x)**: 一時的な障害（Transient Faults）に対する指数バックオフ＋ジッター付き3回リトライ。
  2. **Circuit Breaker**: 連続失敗時に回路を遮断し、外部障害の連鎖（Cascading Failure）を防止。
  3. **Explicit Timeout**: 無期限の待機を防ぐ明示的なタイムアウト設定（`CancellationToken` 連動）。
- Azure SDK 等のクライアントライブラリが組み込みのリトライ機構を備えている場合はそれを適切に構成して活用し、カスタム HTTP やサードパーティ呼び出しには Polly（`ResiliencePipeline`）を使用します。

```csharp
// ResiliencePipeline 構成例 (Composition Root / Infrastructure Adapter)
services.AddResiliencePipeline("ExternalAuthWebhook", builder =>
{
    builder
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(200),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true
        })
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 10,
            BreakDuration = TimeSpan.FromSeconds(15)
        })
        .AddTimeout(TimeSpan.FromSeconds(10));
});
```

