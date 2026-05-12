using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Diagnostics.Internal;

/// <summary>
/// Provides diagnostic metrics for the Vector Store building block.
/// </summary>
[VKBlockDiagnostics<VKAIVectorStoreBlock>]
internal static partial class AIVectorStoreDiagnostics
{
    private static readonly Histogram<double> SearchDuration = Meter!.CreateHistogram<double>(
        "vk_ai_vector_search_duration",
        "seconds",
        "Duration of vector search operations");

    private static readonly Counter<long> RecallHits = Meter!.CreateCounter<long>(
        "vk_ai_vector_recall_hits",
        "hits",
        "Number of successful vector recalls");

    /// <summary>
    /// Records the duration of a search operation.
    /// </summary>
    public static void RecordSearchDuration(double seconds, string? modelId = null)
    {
        SearchDuration.Record(seconds, new TagList { { "model_id", modelId ?? "unknown" } });
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
