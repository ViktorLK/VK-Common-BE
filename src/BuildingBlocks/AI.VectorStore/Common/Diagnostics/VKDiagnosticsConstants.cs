using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Public diagnostic tokens for the AI Vector Store.
/// </summary>
public static class VKDiagnosticsConstants
{
    // Logs (Event IDs)
    public static class Logs
    {
        // No logs currently defined in core VectorStore block
    }

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string SearchDuration = "vk.ai.vector.search_duration";
        public const string RecallHits = "vk.ai.vector.recall_hits";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string ModelId = "model_id";
    }
}
