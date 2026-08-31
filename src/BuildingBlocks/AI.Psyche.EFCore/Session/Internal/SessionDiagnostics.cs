using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore.Session.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]
internal static partial class SessionDiagnostics
{
    [LoggerMessage(EventId = 73701, Level = LogLevel.Error, Message = "Failed to get session entity for SessionId: {SessionId}")]
    public static partial void LogGetSessionEntityError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 73702, Level = LogLevel.Error, Message = "Failed to list session entities")]
    public static partial void LogListSessionEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 73703, Level = LogLevel.Error, Message = "Failed to create session entity for SessionId: {SessionId}")]
    public static partial void LogCreateSessionEntityError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 73704, Level = LogLevel.Error, Message = "Failed to update session entity for SessionId: {SessionId}")]
    public static partial void LogUpdateSessionEntityError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 73705, Level = LogLevel.Error, Message = "Failed to delete session entity for SessionId: {SessionId}")]
    public static partial void LogDeleteSessionEntityError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 73706, Level = LogLevel.Error, Message = "Failed to get session in PsycheSessionStore for SessionId: {SessionId}")]
    public static partial void LogGetSessionError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 73707, Level = LogLevel.Error, Message = "Failed to save session in PsycheSessionStore for SessionId: {SessionId}")]
    public static partial void LogSaveSessionError(this ILogger logger, Exception ex, string sessionId);

    [VKMetricHistogram("vk.ai.psyche.efcore.session.duration", Unit = "ms", Description = "Duration of EFCore Session database operations in milliseconds.")]
    public static partial void RecordSessionOperation(double durationMs, [VKMetricTag("operation")] string operation, [VKMetricTag("success")] bool success);

    [VKMetricCounter("vk.ai.psyche.efcore.session.errors", Unit = "errors", Description = "Total number of EFCore Session database errors.")]
    public static partial void RecordSessionError([VKMetricTag("operation")] string operation);
}
