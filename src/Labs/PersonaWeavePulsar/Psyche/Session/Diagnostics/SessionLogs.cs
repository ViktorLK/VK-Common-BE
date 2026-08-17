using System;
using Microsoft.Extensions.Logging;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Session.Diagnostics;

/// <summary>
/// LoggerMessage Source Generator extension class for Session slice. [OR.01]
/// </summary>
internal static partial class SessionLogs
{
    [LoggerMessage(EventId = 7060, Level = LogLevel.Error, Message = "Failed to get chat session state for session {SessionId}")]
    public static partial void LogGetSessionError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 7061, Level = LogLevel.Error, Message = "Failed to save chat session state for session {SessionId}")]
    public static partial void LogSaveSessionError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 7062, Level = LogLevel.Error, Message = "Failed to get session entity {SessionId}")]
    public static partial void LogGetSessionEntityError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 7063, Level = LogLevel.Error, Message = "Failed to list session entities")]
    public static partial void LogListSessionEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 7064, Level = LogLevel.Error, Message = "Failed to create session entity {SessionId}")]
    public static partial void LogCreateSessionEntityError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 7065, Level = LogLevel.Error, Message = "Failed to update session entity {SessionId}")]
    public static partial void LogUpdateSessionEntityError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 7066, Level = LogLevel.Error, Message = "Failed to delete session entity {SessionId}")]
    public static partial void LogDeleteSessionEntityError(this ILogger logger, Exception ex, string sessionId);
}
