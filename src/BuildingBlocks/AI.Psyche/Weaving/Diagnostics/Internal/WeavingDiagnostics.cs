using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class WeavingDiagnostics
{
    [LoggerMessage(
        EventId = VKWeavingDiagnosticsConstants.Logs.WeavingTruncated,
        Level = LogLevel.Information,
        Message = "Prompt history truncated. SessionId: {SessionId}, BudgetLimit: {Budget}, CurrentTokens: {CurrentTokens}, EvictedCount: {EvictedCount}")]
    public static partial void WeavingTruncated(this ILogger logger, VKSessionId sessionId, int budget, int currentTokens, int evictedCount);

    [LoggerMessage(
        EventId = VKWeavingDiagnosticsConstants.Logs.WeavingAssembled,
        Level = LogLevel.Information,
        Message = "Prompt tapestry assembled. SessionId: {SessionId}, MessageCount: {MessageCount}")]
    public static partial void WeavingAssembled(this ILogger logger, VKSessionId sessionId, int messageCount);

    [LoggerMessage(
        EventId = VKWeavingDiagnosticsConstants.Logs.WeavingEmptyActive,
        Level = LogLevel.Warning,
        Message = "No active prompt fragments remaining after filters. SessionId: {SessionId}")]
    public static partial void WeavingEmptyActive(this ILogger logger, VKSessionId sessionId);
}
