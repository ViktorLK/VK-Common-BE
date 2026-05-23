namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Public diagnostic tokens for the Memory feature.
/// </summary>
public static class VKMemoryDiagnosticTokens
{
    // Logs (Event IDs)
    public const int MemorySummarizationFailedEventId = 300;
    public const int MemoryPrunedEventId = 301;
    public const int MemorySummarizedEventId = 302;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string SummarizationDuration = "vk.ai.cognitive.memory.summarization_duration";
        public const string ContextWindowUtilization = "vk.ai.cognitive.memory.context_window_utilization";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string MemoryCategory = "vk.ai.memory.category";
        public const string IsCompressed = "vk.ai.memory.is_compressed";
    }
}
