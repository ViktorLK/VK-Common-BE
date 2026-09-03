using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.Diagnostics.Internal;

/// <summary>
/// Partial class for resilience diagnostics implementation.
/// Follows [OR.01] and [BB.04].
/// </summary>
[VKBlockDiagnostics<VKResilienceBlock>]
internal static partial class ResilienceDiagnostics
{
    private static readonly Counter<long> _strategyExecutions;

    static ResilienceDiagnostics()
    {
        _strategyExecutions = Meter.CreateCounter<long>(
            VKResilienceDiagnosticsConstants.StrategyExecutionCount,
            "count",
            "Total number of resilience strategy executions");
    }

    internal static Activity? StartActivity(string name) => Source.StartActivity(name);

    internal static void RecordStrategyExecution(string strategyName, bool success)
    {
        _strategyExecutions.Add(1, new TagList
        {
            { "strategy", strategyName },
            { "success", success }
        });
    }

    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Information,
        Message = "Resilience strategy '{Strategy}' executed. Success: {Success}, OperationKey: '{OperationKey}', TraceId: '{TraceId}'")]
    internal static partial void LogStrategyExecuted(
        this ILogger logger,
        string strategy,
        bool success,
        string operationKey,
        string? traceId);

    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Warning,
        Message = "Circuit breaker tripped for key '{Key}' for {DurationSeconds}s. TraceId: '{TraceId}'")]
    internal static partial void LogCircuitBreakerTripped(
        this ILogger logger,
        string key,
        double durationSeconds,
        string? traceId);

    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Information,
        Message = "Circuit breaker reset for key '{Key}'. TraceId: '{TraceId}'")]
    internal static partial void LogCircuitBreakerReset(
        this ILogger logger,
        string key,
        string? traceId);

    [LoggerMessage(
        EventId = 5004,
        Level = LogLevel.Warning,
        Message = "Retry attempt #{Attempt} after {DelayMs}ms delay due to error '{ErrorCode}'. TraceId: '{TraceId}'")]
    internal static partial void LogRetryAttempt(
        this ILogger logger,
        int attempt,
        double delayMs,
        string errorCode,
        string? traceId);

    [LoggerMessage(
        EventId = 5005,
        Level = LogLevel.Warning,
        Message = "Timeout triggered after {DurationMs}ms. TraceId: '{TraceId}'")]
    internal static partial void LogTimeoutTriggered(
        this ILogger logger,
        double durationMs,
        string? traceId);

    [LoggerMessage(
        EventId = 5006,
        Level = LogLevel.Warning,
        Message = "Rate limit exceeded for key '{Key}'. TraceId: '{TraceId}'")]
    internal static partial void LogRateLimitExceeded(
        this ILogger logger,
        string key,
        string? traceId);
}
