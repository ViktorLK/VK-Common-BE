using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.ExceptionHandling.Diagnostics.Internal;

/// <summary>
/// Source-generated logging for the ExceptionHandling building block.
/// </summary>
internal static partial class ExceptionHandlingLog
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "An unhandled exception occurred: {Message}. TraceId: {TraceId}")]
    public static partial void UnhandledException(this ILogger logger, Exception exception, string message, string? traceId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Exception successfully handled by {Handler}. TraceId: {TraceId}")]
    public static partial void ExceptionHandled(this ILogger logger, string handler, string? traceId);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Exception pipeline could not find a suitable handler for {ExceptionType}. TraceId: {TraceId}")]
    public static partial void NoHandlerFound(this ILogger logger, Exception exception, string exceptionType, string? traceId);
}
