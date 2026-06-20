using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIPsycheBlock>]
internal static partial class KnowledgeDiagnostics
{
    // --- 1. [LoggerMessage] Log Message Generators (OR.01) ---

    [LoggerMessage(
        EventId = VKDiagnosticsConstants.Logs.KnowledgeInitialized,
        Level = LogLevel.Information,
        Message = "Knowledge feature initialized for {Name}")]
    public static partial void KnowledgeInitialized(this ILogger logger, string name);

    [LoggerMessage(
        EventId = VKDiagnosticsConstants.Logs.FactArchived,
        Level = LogLevel.Debug,
        Message = "Archived fact with key '{Key}' into the knowledge.")]
    public static partial void FactArchived(this ILogger logger, string key);

    [LoggerMessage(
        EventId = VKDiagnosticsConstants.Logs.LedgerNotImplemented,
        Level = LogLevel.Warning,
        Message = "Knowledge retrieval for key '{Key}' is not yet implemented in this provider.")]
    public static partial void LedgerNotImplemented(this ILogger logger, string key);

    // --- 2. Telemetry Metrics ---

    private static readonly Histogram<double> RetrievalDuration;

    static KnowledgeDiagnostics()
    {
        RetrievalDuration = Meter!.CreateHistogram<double>(
            VKDiagnosticsConstants.Metrics.RetrievalDuration,
            "ms",
            "Duration of knowledge retrieval");
    }

    public static void RecordRetrieval(double milliseconds, string strategy)
    {
        RetrievalDuration.Record(milliseconds, new TagList { { VKDiagnosticsConstants.Tags.SearchStrategy, strategy } });
    }
}
