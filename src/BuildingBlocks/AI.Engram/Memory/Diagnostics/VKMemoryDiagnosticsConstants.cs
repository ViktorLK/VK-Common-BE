namespace VK.Blocks.AI.Engram;

/// <summary>
/// Public diagnostic tokens for the Memory feature.
/// </summary>
public static class VKMemoryDiagnosticsConstants
{
    // Logs (Event IDs)
    public const int MemorySummarizationFailedEventId = 0 + 1;
    public const int MemoryPrunedEventId = 0 + 2;
    public const int MemorySummarizedEventId = 0 + 3;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string SummarizationDuration = "vk.ai.cognitive.memory.summarization_duration";
        public const string ContextWindowUtilization = "vk.ai.cognitive.memory.context_window_utilization";
        public const string MemoryPrunedTotal = "vk.ai.engram.memory.pruned_total";
        public const string RetentionScoreAverage = "vk.ai.engram.memory.retention_score_average";
        public const string ConsolidationLatency = "vk.ai.engram.memory.consolidation_latency";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string MemoryCategory = "vk.ai.memory.category";
        public const string IsCompressed = "vk.ai.memory.is_compressed";
        public const string TenantId = "vk.tenant.id";
    }
}
