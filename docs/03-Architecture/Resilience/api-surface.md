# API Surface & Compatibility Contract: Resilience Subsystem

- **Scope**: `VK.Blocks.Resilience`, `VK.Blocks.Resilience.Polly`
- **Specification Type**: Level 1 Public Surface & SemVer Boundary (`ARCH-07`, `AP.03`, `FS-09`)
- **Status**: ✅ Active

---

## 1. 📜 Public API Surface (Level 1 公開契約)

以下のシンボルのみが、外部モジュールおよびアプリケーション層に対して意図的に公開されている Level 1 公開契約です。これら以外の具象実装（`Internal/` 配下）への直接アクセスは固く禁止されています。

### 1.1 中核プロトコル (`Common/Protocols/`)

```csharp
namespace VK.Blocks.Resilience;

public interface IVKResilienceExecutor
{
    // Task<T> 返却 (CS.03 に従い ValueTask を公に露出させない)
    Task<T> ExecuteAsync<T>(
        string pipelineName,
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default);

    Task<T> ExecuteAsync<T>(
        string pipelineName,
        Func<CancellationToken, ValueTask<T>> action,
        CancellationToken cancellationToken = default);

    // VKResult<T> 透過実行 (無例外制御流)
    Task<VKResult<T>> ExecuteResultAsync<T>(
        string pipelineName,
        Func<CancellationToken, Task<VKResult<T>>> action,
        CancellationToken cancellationToken = default);
}
```

### 1.2 定数・プロファイル・エラー定義 (`Common/Constants/`, `Common/Errors/`)

```csharp
namespace VK.Blocks.Resilience;

public static class VKResiliencePipelines
{
    public const string Default = "VK.Resilience.Default";
    public const string Http = "VK.Resilience.Http";
    public const string Database = "VK.Resilience.Database";
    public const string Cache = "VK.Resilience.Cache";
}

public static class VKResilienceErrors
{
    public static readonly VKError Timeout;
    public static readonly VKError ExecutionFailed;
    public static readonly VKError RetryExhausted;
    public static readonly VKError FallbackFailed;
    public static readonly VKError CircuitBreakerOpen;
    public static readonly VKError RateLimitExceeded;
    public static readonly VKError BulkheadExceeded;
    public static readonly VKError ConcurrencyRejected;
    public static readonly VKError PipelineNotFound;
}
```

### 1.3 構成オプションレコード (`Options/`)

すべてのオプションは `sealed partial record : IVKBlockOptions` であり、不変性と起動時検証を強制します（`BB.05`, `AP.04`）：
- `VKRetryOptions`: `MaxRetries`, `InitialDelay`, `MaxDelay`, `BackoffMultiplier`, `UseJitter`
- `VKTimeoutOptions`: `Timeout`
- `VKCircuitBreakerOptions`: `FailureRatio`, `SamplingDuration`, `DurationOfBreak`, `MinimumThroughput`
- `VKRateLimitOptions`: `PermitLimit`, `QueueLimit`, `Window`
- `VKConcurrencyOptions`: `MaxConcurrency`, `MaxQueueLength`
- `VKFallbackOptions`: `FallbackAction`
- `VKHedgingOptions`: `MaxHedgedAttempts`, `Delay`

### 1.4 DI 登録拡張

- `services.AddVKResilienceBlock(configuration, transform)`
- `services.AddVKResiliencePollyBlock(configuration, transform)`

---

## 2. 🔒 Internal Boundary (内部完全隠蔽領域)

以下のコンポーネントは **`internal sealed`** としてカプセル化されており、外部に漏洩してはなりません：
- `VK.Blocks.Resilience.Common.Internal.NoOpResilienceExecutor`
- `VK.Blocks.Resilience.Polly.Internal.PollyResilienceExecutor`
- `VK.Blocks.Resilience.Polly.Internal.PollyStrategyMapper`
- `VK.Blocks.Resilience.Polly.Internal.VKResilienceTransientFailureException`

---

## 3. ⚖️ SemVer 互換性・破壊的変更ルール (`FS-09`)

| 変更種別 | 判定 | 許容基準と移行プロセス |
| :--- | :---: | :--- |
| `IVKResilienceExecutor` へのメソッド追加 | **破壊的 (Major)** | C# Default Interface Method (DIM) を用いて後方互換デフォルト実装を提供可能な場合を除き、メジャーバージョン改定と ADR 承認が必要（`DL.03`）。 |
| `VKResilienceErrors` エラーコード文字列の変更 | **破壊的 (Major)** | ワイヤー契約（RFC 7807）に影響するため、公開済みエラーコードの文字列リネームは禁止。 |
| `VK*Options` へのプロパティ追加 | **安全 (Minor)** | 必ずデフォルト値を持つ `init` プロパティとして追加し、既存構成を破壊しないこと。 |
| パイプライン名定数の追加 | **安全 (Minor)** | 加法的変更（Additive）としてマイナーバージョンで許容。 |

