using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Afferent;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Text.Diagnostics.Internal;

/// <summary>
/// Source-generated high-performance loggers and diagnostics for the Text slice.
/// Follows OR.01.
/// </summary>
[VKBlockDiagnostics<VKAIAfferentBlock>]
internal static partial class AfferentTextDiagnostics
{
    [LoggerMessage(
        EventId = VKAfferentTextDiagnosticTokens.TextPipelineStartedEventId,
        Level = LogLevel.Information,
        Message = "Afferent Text stage initiated for TenantId: {TenantId}, UserId: {UserId}.")]
    public static partial void TextPipelineStarted(ILogger logger, string tenantId, string userId);

    [LoggerMessage(
        EventId = VKAfferentTextDiagnosticTokens.TextPipelineCompletedEventId,
        Level = LogLevel.Information,
        Message = "Afferent Text stage successfully completed. Original Length: {OriginalLength}, Processed Length: {ProcessedLength}.")]
    public static partial void TextPipelineCompleted(ILogger logger, int originalLength, int processedLength);
}
