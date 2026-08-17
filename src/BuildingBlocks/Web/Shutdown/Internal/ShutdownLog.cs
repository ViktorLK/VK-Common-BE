using Microsoft.Extensions.Logging;

namespace VK.Blocks.Web.Shutdown.Internal;

/// <summary>
/// Logger messages for Graceful Shutdown operations.
/// Complies with OR.01.
/// </summary>
internal static partial class ShutdownLog
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Rejecting request: host is stopping. Path: {Path}")]
    public static partial void LogRequestRejectedStopping(
        this ILogger logger,
        string path);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Graceful shutdown: waiting for {ActiveRequests} active request(s) to drain...")]
    public static partial void LogWaitingForRequestsToDrain(
        this ILogger logger,
        int activeRequests);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Graceful shutdown completed. All request(s) drained.")]
    public static partial void LogDrainingCompleted(
        this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Graceful shutdown: timed out waiting for request(s) to drain. {ActiveRequests} request(s) remaining.")]
    public static partial void LogDrainingTimedOut(
        this ILogger logger,
        int activeRequests);
}
