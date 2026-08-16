using System;
using Microsoft.Extensions.Logging;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Echo.Diagnostics;

/// <summary>
/// LoggerMessage Source Generator extension class for Echo trace slice. [OR.01]
/// </summary>
internal static partial class EchoLogs
{
    [LoggerMessage(EventId = 7070, Level = LogLevel.Error, Message = "Failed to get chat history for session {SessionId}")]
    public static partial void LogGetChatHistoryError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 7071, Level = LogLevel.Error, Message = "Failed to save echo trace {TraceId} for session {SessionId}")]
    public static partial void LogSaveEchoTraceError(this ILogger logger, Exception ex, string traceId, string sessionId);

    [LoggerMessage(EventId = 7072, Level = LogLevel.Error, Message = "Failed to create echo entity {TraceId}")]
    public static partial void LogCreateEchoEntityError(this ILogger logger, Exception ex, string traceId);

    [LoggerMessage(EventId = 7073, Level = LogLevel.Error, Message = "Failed to update echo entity {TraceId}")]
    public static partial void LogUpdateEchoEntityError(this ILogger logger, Exception ex, string traceId);

    [LoggerMessage(EventId = 7074, Level = LogLevel.Error, Message = "Failed to delete echo entity {TraceId}")]
    public static partial void LogDeleteEchoEntityError(this ILogger logger, Exception ex, string traceId);

    [LoggerMessage(EventId = 7075, Level = LogLevel.Error, Message = "Failed to clear history for session {SessionId}")]
    public static partial void LogClearHistoryError(this ILogger logger, Exception ex, string sessionId);
}
