using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.Common.Diagnostics.Internal;

/// <summary>
/// Provides diagnostic metrics for the Vector Store building block.
/// </summary>
[VKBlockDiagnostics<VKVectorStoreBlock>]
internal static partial class VectorStoreDiagnostics
{
    private static readonly Histogram<double> SearchDuration;
    private static readonly Counter<long> RecallHits;

    static VectorStoreDiagnostics()
    {
        SearchDuration = Meter!.CreateHistogram<double>(
            VKDiagnosticsConstants.Metrics.SearchDuration,
            "seconds",
            "Duration of vector search operations");

        RecallHits = Meter!.CreateCounter<long>(
            VKDiagnosticsConstants.Metrics.RecallHits,
            "hits",
            "Number of successful vector recalls");
    }

    /// <summary>
    /// Records the duration of a search operation.
    /// </summary>
    public static void RecordSearchDuration(double seconds, string? modelId = null)
    {
        SearchDuration.Record(seconds, new TagList { { VKDiagnosticsConstants.Tags.ModelId, modelId ?? "unknown" } });
    }

    /// <summary>
    /// Records a recall hit if the score exceeds the threshold.
    /// </summary>
    public static void RecordRecallHit(bool isHit)
    {
        if (isHit)
            RecallHits.Add(1);
    }
}
