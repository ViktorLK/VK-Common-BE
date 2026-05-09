using Microsoft.Extensions.Logging;

namespace VK.Blocks.Web.CorrelationId.Internal;

/// <summary>
/// Logger messages for Correlation ID operations.
/// </summary>
internal static partial class CorrelationIdLog
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Correlation ID '{CorrelationId}' assigned to request {Method} {Path}")]
    public static partial void LogCorrelationIdAssigned(
        this ILogger logger,
        string correlationId,
        string method,
        string path);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "Using existing Correlation ID '{CorrelationId}' for request {Method} {Path}")]
    public static partial void LogCorrelationIdRetrieved(
        this ILogger logger,
        string correlationId,
        string method,
        string path);
}
