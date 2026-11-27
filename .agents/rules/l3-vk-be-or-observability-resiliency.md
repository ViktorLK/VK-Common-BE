---
trigger: manual
---

<ruleset category="OR" name="Observability & Resiliency">

<rule id="OR.01" type="A" name="Observability">
#### Logging
- **Pattern & Enforcement**: USE `[LoggerMessage]` Source Generator on `internal static partial class` extension methods (`this ILogger logger`). Direct `logger.LogXxx()` calls are PROHIBITED *(Enforced: `CA1848`; duplicate EventId: `VK1010`)*.
- **Single File Consolidation**: Consolidate all `[LoggerMessage]` methods for a feature in `{FeatureName}Diagnostics.cs` marked with `[VKBlockDiagnostics<TBlock>]` under `{FeatureName}/Diagnostics/Internal/` (or `Common/Diagnostics/Internal/` for shared loggers); do NOT create parallel `XxxLog.cs` files.
- **Structured Templates & TraceId**: USE structured templates (`"{Id}"`, `"{TenantId}"`), NO string interpolation. `TraceId` is MANDATORY in all log entries and error responses; log exceptions with full context before mapping to `Result<T>`.

#### Metrics & Tracing
- **Diagnostics Class**: Each BuildingBlock MUST define `[VKBlockDiagnostics]` in a `Diagnostics/` folder to auto-generate `ActivitySource` and `Meter`.
- **Constants**: ALL metric names and tag keys MUST be defined in `XxxDiagnosticsConstants.cs`. Follow OpenTelemetry Semantic Conventions.
- **Instrument Selection**: Use `Counter` for counts, `Histogram` for durations, `UpDownCounter` for gauges. NEVER create metrics instruments inside loops.
</rule>

<rule id="OR.02" type="B" name="Security">
- `TenantId` filtering MUST be enforced via EF Core Global Query Filters.
- NO query is allowed to bypass tenant isolation.
- ALL PII and secrets MUST be masked in logs via a dedicated masking processor (e.g. `SensitiveDataProcessor`).
</rule>

<rule id="OR.03" type="B" name="Resiliency">
- ALL external calls (HTTP / Azure SDK / third-party) MUST be wrapped with Polly.
- Minimum policy: Retry (3x) + CircuitBreaker.
- Timeout MUST be explicitly configured. NO indefinite waits.
- Prefer using Azure SDK's built-in retries when applicable, and use Polly for custom HTTP/Third-party calls.
</rule>

</ruleset>
