using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Diagnostics.Internal;

/// <summary>
/// Source-generated logger messages and metrics for Psyche Weaving stage.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class WeavingDiagnostics
{
    // --- Source Generated Metrics (v1.5) ---

    [VKMetricHistogram(
        VKWeavingDiagnosticsConstants.Metrics.WeavingDuration,
        Unit = "ms",
        Description = "Duration of prompt assembly and weaving in milliseconds.")]
    public static partial void RecordWeaving(
        double durationMs,
        [VKMetricTag(VKWeavingDiagnosticsConstants.Tags.StageName)] string stage,
        [VKMetricTag(VKPsycheDiagnosticsConstants.Tags.IsSuccess)] bool success);

    [VKMetricCounter(
        VKWeavingDiagnosticsConstants.Metrics.TokensAssembled,
        Unit = "tokens",
        Description = "Total tokens or messages successfully woven into prompt tapestry.")]
    public static partial void RecordTokensAssembled(
        long count,
        [VKMetricTag(VKWeavingDiagnosticsConstants.Tags.StageName)] string stage);

    [VKMetricCounter(
        VKWeavingDiagnosticsConstants.Metrics.TokensBudgetExceeded,
        Unit = "events",
        Description = "Number of times conversation history or fragments exceeded budget limit.")]
    public static partial void RecordTruncation(
        long evictedCount,
        [VKMetricTag(VKWeavingDiagnosticsConstants.Tags.StageName)] string stage,
        [VKMetricTag(VKWeavingDiagnosticsConstants.Tags.Budget)] int budget);

    // --- [LoggerMessage] Generators (OR.01) ---

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
