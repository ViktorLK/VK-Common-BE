using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Engram.Retrieval.Diagnostics.Internal;

internal static partial class RetrievalLogExtensions
{
    [LoggerMessage(
        EventId = 4001,
        Level = LogLevel.Error,
        Message = "Failed to prefetch predictive context for cue: {Cue}")]
    public static partial void PrefetchError(this ILogger logger, Exception exception, string cue);

    [LoggerMessage(
        EventId = 4002,
        Level = LogLevel.Debug,
        Message = "Intent Cue extraction timed out after {TimeoutMs}ms. Falling back to Safety Net.")]
    public static partial void PrefetchIntentExtractionTimeout(this ILogger logger, int timeoutMs);
}
