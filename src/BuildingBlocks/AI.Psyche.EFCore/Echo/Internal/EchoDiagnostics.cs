using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore.Echo.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]
internal static partial class EchoDiagnostics
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to get echo entity for EchoId: {EchoId}")]
    public static partial void LogGetEchoEntityError(this ILogger logger, Exception ex, string echoId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to get echo entities for SessionId: {SessionId}")]
    public static partial void LogGetEchoBySessionIdError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to create echo entity for EchoId: {EchoId}")]
    public static partial void LogCreateEchoEntityError(this ILogger logger, Exception ex, string echoId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Failed to update echo entity for EchoId: {EchoId}")]
    public static partial void LogUpdateEchoEntityError(this ILogger logger, Exception ex, string echoId);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Failed to delete echo entity for EchoId: {EchoId}")]
    public static partial void LogDeleteEchoEntityError(this ILogger logger, Exception ex, string echoId);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Failed to get history in PsycheEchoStore for SessionId: {SessionId}")]
    public static partial void LogGetHistoryStoreError(this ILogger logger, Exception ex, string sessionId);

    [LoggerMessage(EventId = 7, Level = LogLevel.Error, Message = "Failed to save history in PsycheEchoStore for EchoId: {EchoId}")]
    public static partial void LogSaveHistoryStoreError(this ILogger logger, Exception ex, string echoId);
}
