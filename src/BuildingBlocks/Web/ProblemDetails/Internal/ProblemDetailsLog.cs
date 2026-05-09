using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.Web.ProblemDetails.Internal;

/// <summary>
/// Source Generated loggers for the ProblemDetails feature.
/// Complies with OR.01 (Logging Pattern).
/// </summary>
internal static partial class ProblemDetailsLog
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "An unhandled exception occurred during request processing: {Message}")]
    public static partial void LogUnhandledException(ILogger logger, Exception exception, string message);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Created ProblemDetails response. Code: {ErrorCode}, Type: {VKErrorType}, Status: {StatusCode}")]
    public static partial void LogProblemDetailsCreated(ILogger logger, string? errorCode, string vkErrorType, int? statusCode);
}

