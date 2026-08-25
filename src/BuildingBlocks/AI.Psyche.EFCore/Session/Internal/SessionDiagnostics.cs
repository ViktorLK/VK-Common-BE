using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore.Session.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]
internal static partial class SessionDiagnostics
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to get session entity for SessionId: {SessionId}")]
    public static partial void LogGetSessionEntityError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to list session entities")]
    public static partial void LogListSessionEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to create session entity for SessionId: {SessionId}")]
    public static partial void LogCreateSessionEntityError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Failed to update session entity for SessionId: {SessionId}")]
    public static partial void LogUpdateSessionEntityError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Failed to delete session entity for SessionId: {SessionId}")]
    public static partial void LogDeleteSessionEntityError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Failed to get session in PsycheSessionStore for SessionId: {SessionId}")]
    public static partial void LogGetSessionError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 7, Level = LogLevel.Error, Message = "Failed to save session in PsycheSessionStore for SessionId: {SessionId}")]
    public static partial void LogSaveSessionError(this ILogger logger, Exception ex, string sessionId);
}
