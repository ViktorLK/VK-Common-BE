using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Diagnostics.Internal;

/// <summary>
/// Provides diagnostic metrics for the Vector Store building block.
/// </summary>
[VKBlockDiagnostics<VKAIVectorStoreBlock>]
public static partial class VKAIVectorStoreDiagnostics
{
    private static readonly Histogram<double> SearchDuration;
    private static readonly Counter<long> RecallHits;

    static VKAIVectorStoreDiagnostics()
    {
        SearchDuration = Meter!.CreateHistogram<double>(
            VKAIVectorStoreDiagnosticsConstants.Metrics.SearchDuration,
            "seconds",
            "Duration of vector search operations");

        RecallHits = Meter!.CreateCounter<long>(
            VKAIVectorStoreDiagnosticsConstants.Metrics.RecallHits,
            "hits",
            "Number of successful vector recalls");
    }

    /// <summary>
    /// Records the duration of a search operation.
    /// </summary>
    public static void RecordSearchDuration(double seconds, string? modelId = null)
    {
        SearchDuration.Record(seconds, new TagList { { VKAIVectorStoreDiagnosticsConstants.Tags.ModelId, modelId ?? "unknown" } });
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
