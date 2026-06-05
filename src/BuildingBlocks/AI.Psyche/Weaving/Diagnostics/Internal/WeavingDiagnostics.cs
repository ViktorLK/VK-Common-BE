using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class WeavingDiagnostics
{
    [LoggerMessage(
        EventId = VKWeavingDiagnostics.WeavingTruncatedEventId,
        Level = LogLevel.Information,
        Message = "Prompt history truncated. SessionId: {SessionId}, BudgetLimit: {Budget}, CurrentTokens: {CurrentTokens}, EvictedCount: {EvictedCount}")]
    public static partial void WeavingTruncated(ILogger logger, VKSessionId sessionId, int budget, int currentTokens, int evictedCount);

    [LoggerMessage(
        EventId = VKWeavingDiagnostics.WeavingAssembledEventId,
        Level = LogLevel.Information,
        Message = "Prompt tapestry assembled. SessionId: {SessionId}, MessageCount: {MessageCount}")]
    public static partial void WeavingAssembled(ILogger logger, VKSessionId sessionId, int messageCount);

    [LoggerMessage(
        EventId = VKWeavingDiagnostics.WeavingEmptyActiveEventId,
        Level = LogLevel.Warning,
        Message = "No active prompt fragments remaining after filters. SessionId: {SessionId}")]
    public static partial void WeavingEmptyActive(ILogger logger, VKSessionId sessionId);
}
