using Microsoft.Extensions.Logging;

namespace VK.Blocks.Observability.Diagnostics.Internal;

/// <summary>
/// Source-generated logging extensions for the Observability Core block.
/// Complies with OR.01.
/// </summary>
internal static partial class ObservabilityLog
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Debug,
        Message = "Observability block services registered.")]
    public static partial void LogBlockRegistered(this ILogger logger);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Operation '{OperationName}' exceeded slow threshold: {ElapsedMs}ms > {ThresholdMs}ms.")]
    public static partial void LogSlowOperation(this ILogger logger, string operationName, double elapsedMs, double thresholdMs);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Slow query detected: {ElapsedMs}ms > {ThresholdMs}ms.")]
    public static partial void LogSlowQuery(this ILogger logger, double elapsedMs, double thresholdMs);
}
