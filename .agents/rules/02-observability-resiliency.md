---
trigger: always_on
---

# VK.Blocks: Observability & Resiliency

### Rule 6 — Observability

#### Logging

- **Pattern**: USE `[LoggerMessage]` Source Generator (SG) for ALL logging. Define as `internal static partial class` with extension methods on `ILogger`.
- **Enforcement**: DIRECT calling of standard logger methods (e.g. `logger.LogInformation()`, `logger.LogWarning()`, `logger.LogError()`) is PROHIBITED in production code.
- **Structured Templates**: USE structured log templates with placeholders: `"{Id}"`, `"{TenantId}"`. NO string interpolation.
- **TraceId**: `TraceId` is MANDATORY in all log entries and error responses.
- **Exception Context**: Exceptions MUST be logged with full context before mapping to `Result<T>`.
- **Location**: Feature-specific loggers MUST be placed in `{FeatureName}/Internal/` (e.g. `Permissions/Internal/PermissionsLog.cs`). Only globally shared or infrastructure-level loggers belong in `Diagnostics/Internal/`.

#### Metrics & Tracing

- **Diagnostics Class**: Each BuildingBlock MUST define `[VKBlockDiagnostics]` in a `Diagnostics/` folder to auto-generate `ActivitySource` and `Meter`.
- **Constants**: ALL metric names and tag keys MUST be defined in `XxxDiagnosticsConstants.cs`. Follow OpenTelemetry Semantic Conventions.
- **Instrument Selection**: Use `Counter` for counts, `Histogram` for durations, `UpDownCounter` for gauges. NEVER create metrics instruments inside loops.

### Rule 7 — Security

- `TenantId` filtering MUST be enforced via EF Core Global Query Filters.
- NO query is allowed to bypass tenant isolation.
- ALL PII and secrets MUST be masked in logs via a dedicated masking processor (e.g. `SensitiveDataProcessor`).

### Rule 8 — Resiliency

- ALL external calls (HTTP / Azure SDK / third-party) MUST be wrapped with Polly.
- Minimum policy: Retry (3x) + CircuitBreaker.
- Timeout MUST be explicitly configured. NO indefinite waits.
- Prefer using Azure SDK's built-in retries when applicable, and use Polly for custom HTTP/Third-party calls.
