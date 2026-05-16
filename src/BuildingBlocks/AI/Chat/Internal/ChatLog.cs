using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Chat.Internal;

/// <summary>
/// Source-generated logger messages for the Chat feature.
/// </summary>
internal static partial class ChatLog
{
    [LoggerMessage(
        EventId = 700,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Chat started request: {Input}")]
    public static partial void ChatRequestStarted(ILogger logger, string tenantId, string traceId, string input);

    [LoggerMessage(
        EventId = 701,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Chat completed request. Role: {Role}, Tokens: {Tokens}")]
    public static partial void ChatRequestCompleted(ILogger logger, string tenantId, string traceId, string role, int tokens);

    [LoggerMessage(
        EventId = 702,
        Level = LogLevel.Warning,
        Message = "{TenantId} {TraceId} [AI] Chat request failed: {Error}")]
    public static partial void ChatRequestFailed(ILogger logger, string tenantId, string traceId, string error);

    [LoggerMessage(
        EventId = 703,
        Level = LogLevel.Warning,
        Message = "{TenantId} {TraceId} [AI] Chat request timed out after {TimeoutMs}ms")]
    public static partial void ChatRequestTimedOut(ILogger logger, string tenantId, string traceId, double timeoutMs);

    [LoggerMessage(
        EventId = 704,
        Level = LogLevel.Error,
        Message = "{TenantId} {TraceId} [AI] Chat encountered an unexpected error.")]
    public static partial void UnexpectedChatError(ILogger logger, string tenantId, string traceId, System.Exception ex);
}
