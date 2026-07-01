using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Afferent;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressText.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIAfferentBlock>]
internal static partial class IngressTextDiagnostics
{
    [LoggerMessage(
        EventId = VKIngressTextDiagnosticTokens.TextPipelineStartedEventId,
        Level = LogLevel.Information,
        Message = "Ingress Text stage initiated for TenantId: {TenantId}, UserId: {UserId}.")]
    public static partial void TextPipelineStarted(ILogger logger, string tenantId, string userId);

    [LoggerMessage(
        EventId = VKIngressTextDiagnosticTokens.TextPipelineCompletedEventId,
        Level = LogLevel.Information,
        Message = "Ingress Text stage successfully completed. Original Length: {OriginalLength}, Processed Length: {ProcessedLength}.")]
    public static partial void TextPipelineCompleted(ILogger logger, int originalLength, int processedLength);
}
