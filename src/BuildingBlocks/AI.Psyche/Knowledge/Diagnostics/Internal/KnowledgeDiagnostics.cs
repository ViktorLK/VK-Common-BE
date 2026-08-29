using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Diagnostics.Internal;

/// <summary>
/// Source-generated logger messages and metrics for Psyche Knowledge stage.
/// Follows BB.04 and OR.01.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class KnowledgeDiagnostics
{
    // --- Source Generated Metrics (v1.5) ---

    [VKMetricHistogram(
        VKKnowledgeDiagnosticsConstants.Metrics.RetrievalDuration,
        Unit = "ms",
        Description = "Duration of knowledge retrieval and rule matching in milliseconds.")]
    public static partial void RecordRetrieval(
        double milliseconds,
        [VKMetricTag(VKKnowledgeDiagnosticsConstants.Tags.StageName)] string stage,
        [VKMetricTag(VKKnowledgeDiagnosticsConstants.Tags.SearchStrategy)] string strategy);

    [VKMetricCounter(
        VKKnowledgeDiagnosticsConstants.Metrics.EntriesMatched,
        Unit = "entries",
        Description = "Total number of knowledge entries matched and activated for prompt context.")]
    public static partial void RecordEntriesMatched(
        long count,
        [VKMetricTag(VKKnowledgeDiagnosticsConstants.Tags.StageName)] string stage,
        [VKMetricTag(VKKnowledgeDiagnosticsConstants.Tags.SearchStrategy)] string strategy);

    // --- [LoggerMessage] Generators (OR.01) ---

    [LoggerMessage(
        EventId = VKKnowledgeDiagnosticsConstants.Logs.KnowledgeInitialized,
        Level = LogLevel.Information,
        Message = "Knowledge feature initialized for {Name}")]
    public static partial void KnowledgeInitialized(this ILogger logger, string name);

    [LoggerMessage(
        EventId = VKKnowledgeDiagnosticsConstants.Logs.FactArchived,
        Level = LogLevel.Debug,
        Message = "Archived fact with key '{Key}' into the knowledge.")]
    public static partial void FactArchived(this ILogger logger, string key);

    [LoggerMessage(
        EventId = VKKnowledgeDiagnosticsConstants.Logs.LedgerNotImplemented,
        Level = LogLevel.Warning,
        Message = "Knowledge retrieval for key '{Key}' is not yet implemented in this provider.")]
    public static partial void LedgerNotImplemented(this ILogger logger, string key);

    [LoggerMessage(
        EventId = VKKnowledgeDiagnosticsConstants.Logs.KnowledgeMatched,
        Level = LogLevel.Information,
        Message = "Knowledge stage matched {Count} entries for Session: {SessionId}, CorrelationId: {CorrelationId}, Duration: {DurationMs}ms")]
    public static partial void KnowledgeMatched(this ILogger logger, int count, string sessionId, string correlationId, double durationMs);
}
