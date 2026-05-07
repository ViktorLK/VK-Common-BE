# Standard 02: Observability & Resiliency

## 1. Logging (Structured & High Performance)
- **Source Generation**: Use `[LoggerMessage]` source generator for all production logging.
- **Internal Static Classes**: Loggers must be `internal static partial class`.
- **No Interpolation**: Use structured templates (e.g., `"{UserId}"`) instead of string interpolation.
- **Masking**: PII and secrets must be masked via `SensitiveDataProcessor` (OR.02).

## 2. Metrics & Tracing
- **ActivitySource**: Every module must use its generated `ActivitySource`.
- **OTEL Conventions**: Follow OpenTelemetry Semantic Conventions for tag naming.
- **Diagnostics Class**: Annotated with `[VKBlockDiagnostics]`.

## 3. Resiliency (Polly)
ALL I/O and external integration calls MUST be wrapped in a resiliency policy.
- **Minimum Policy**: Retry (3x) + Circuit Breaker.
- **Timeout**: Mandatory explicit timeout (default 30s unless specified).
- **Fallback**: Implement fallback logic where business-critical (e.g., returning cached data if DB is down).

## 4. Implementation Example
```csharp
internal static partial class AuthLogs
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "ApiKey {KeyId} authentication failed.")]
    public static partial void AuthFailed(this ILogger logger, string keyId);
}
```

