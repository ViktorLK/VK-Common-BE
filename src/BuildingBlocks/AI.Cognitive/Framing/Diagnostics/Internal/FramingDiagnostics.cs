using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Cognitive;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Framing.Diagnostics.Internal;

/// <summary>
/// Source-generated high-performance loggers and diagnostics for the Framing slice.
/// Follows OR.01.
/// </summary>
[VKBlockDiagnostics<VKAICognitiveBlock>]
internal static partial class FramingDiagnostics
{
    [LoggerMessage(
        EventId = VKFramingDiagnosticTokens.FramingPipelineStartedEventId,
        Level = LogLevel.Information,
        Message = "Cognitive Framing stage initiated for TenantId: {TenantId}, UserId: {UserId}.")]
    public static partial void FramingPipelineStarted(ILogger logger, string tenantId, string userId);

    [LoggerMessage(
        EventId = VKFramingDiagnosticTokens.FramingPipelineCompletedEventId,
        Level = LogLevel.Information,
        Message = "Cognitive Framing stage successfully completed. Reserved System Tokens: {ReservedTokens}, Available History Limit: {HistoryLimit}.")]
    public static partial void FramingPipelineCompleted(ILogger logger, int reservedTokens, int historyLimit);

    [LoggerMessage(
        EventId = VKFramingDiagnosticTokens.FramingFallbackTriggeredEventId,
        Level = LogLevel.Warning,
        Message = "AzureOpenAI stress detected. Cognitive Framing fallback timeout triggered. Overridden to: {TimeoutSeconds}s.")]
    public static partial void FramingFallbackTriggered(ILogger logger, double timeoutSeconds);
}
