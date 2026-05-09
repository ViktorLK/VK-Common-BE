using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.Web.RequestLogging.Internal;

/// <summary>
/// Logger messages for standard Web request processing.
/// Complies with OR.01 (Observability) for structured, allocation-free logging.
/// </summary>
internal static partial class RequestLoggingLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "HTTP {Method} {Path} started. CorrelationId: {CorrelationId}")]
    public static partial void LogRequestStarted(
        this ILogger logger,
        string method,
        string path,
        string? correlationId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs:F4}ms")]
    public static partial void LogRequestCompleted(
        this ILogger logger,
        string method,
        string path,
        int statusCode,
        double elapsedMs);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "HTTP {Method} {Path} failed after {ElapsedMs:F4}ms")]
    public static partial void LogRequestFailed(
        this ILogger logger,
        Exception exception,
        string method,
        string path,
        double elapsedMs);
}

