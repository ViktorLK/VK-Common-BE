using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore.Echo.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]
internal static partial class EchoDiagnostics
{
    [LoggerMessage(EventId = 73501, Level = LogLevel.Error, Message = "Failed to get echo entity for EchoId: {EchoId}")]
    public static partial void LogGetEchoEntityError(this ILogger logger, Exception ex, string echoId);

    [LoggerMessage(EventId = 73502, Level = LogLevel.Error, Message = "Failed to get echo entities for SessionId: {SessionId}")]
    public static partial void LogGetEchoBySessionIdError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 73503, Level = LogLevel.Error, Message = "Failed to create echo entity for EchoId: {EchoId}")]
    public static partial void LogCreateEchoEntityError(this ILogger logger, Exception ex, string echoId);

    [LoggerMessage(EventId = 73504, Level = LogLevel.Error, Message = "Failed to update echo entity for EchoId: {EchoId}")]
    public static partial void LogUpdateEchoEntityError(this ILogger logger, Exception ex, string echoId);

    [LoggerMessage(EventId = 73505, Level = LogLevel.Error, Message = "Failed to delete echo entity for EchoId: {EchoId}")]
    public static partial void LogDeleteEchoEntityError(this ILogger logger, Exception ex, string echoId);

    [LoggerMessage(EventId = 73506, Level = LogLevel.Error, Message = "Failed to get history in PsycheEchoStore for SessionId: {SessionId}")]
    public static partial void LogGetHistoryStoreError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 73507, Level = LogLevel.Error, Message = "Failed to save history in PsycheEchoStore for EchoId: {EchoId}")]
    public static partial void LogSaveHistoryStoreError(this ILogger logger, Exception ex, string echoId);

    [VKMetricHistogram("vk.ai.psyche.efcore.echo.duration", Unit = "ms", Description = "Duration of EFCore Echo database operations in milliseconds.")]
    public static partial void RecordEchoOperation(double durationMs, [VKMetricTag("operation")] string operation, [VKMetricTag("success")] bool success);

    [VKMetricCounter("vk.ai.psyche.efcore.echo.errors", Unit = "errors", Description = "Total number of EFCore Echo database errors.")]
    public static partial void RecordEchoError([VKMetricTag("operation")] string operation);
}
