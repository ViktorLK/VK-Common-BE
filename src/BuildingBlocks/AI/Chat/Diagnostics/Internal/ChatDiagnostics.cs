using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Chat.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class ChatDiagnostics
{
    [LoggerMessage(
        EventId = VKChatDiagnosticsConstants.ChatInitializedEventId,
        Level = LogLevel.Debug,
        Message = "Chat feature initialized.")]
    public static partial void ChatInitialized(ILogger logger);

    [LoggerMessage(
        EventId = VKChatDiagnosticsConstants.ChatRequestStartedEventId,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Chat started request: {Input}")]
    public static partial void ChatRequestStarted(ILogger logger, string tenantId, string traceId, string input);

    /// <summary>
    /// Masks user input to prevent PII leakage in logs.
    /// </summary>
    public static string MaskInput(string? input)
    {
        if (input is null)
        {
            return string.Empty;
        }

        return input.Length > 100 ? $"{input[..100]}... [MASKED]" : input;
    }

    [LoggerMessage(
        EventId = VKChatDiagnosticsConstants.ChatRequestCompletedEventId,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Chat completed request. Role: {Role}, Tokens: {Tokens}")]
    public static partial void ChatRequestCompleted(ILogger logger, string tenantId, string traceId, string role, int tokens);

    [LoggerMessage(
        EventId = VKChatDiagnosticsConstants.ChatRequestFailedEventId,
        Level = LogLevel.Warning,
        Message = "{TenantId} {TraceId} [AI] Chat request failed: {Error}")]
    public static partial void ChatRequestFailed(ILogger logger, string tenantId, string traceId, string error);

    [LoggerMessage(
        EventId = VKChatDiagnosticsConstants.ChatRequestTimedOutEventId,
        Level = LogLevel.Warning,
        Message = "{TenantId} {TraceId} [AI] Chat request timed out after {TimeoutMs}ms")]
    public static partial void ChatRequestTimedOut(ILogger logger, string tenantId, string traceId, double timeoutMs);

    [LoggerMessage(
        EventId = VKChatDiagnosticsConstants.UnexpectedChatErrorEventId,
        Level = LogLevel.Error,
        Message = "{TenantId} {TraceId} [AI] Chat encountered an unexpected error.")]
    public static partial void UnexpectedChatError(ILogger logger, string tenantId, string traceId, System.Exception ex);
}
