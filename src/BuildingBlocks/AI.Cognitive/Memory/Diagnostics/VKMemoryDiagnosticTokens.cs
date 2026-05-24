using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Public diagnostic tokens for the Memory feature.
/// </summary>
public static class VKMemoryDiagnosticTokens
{
    // Logs (Event IDs)
    public const int MemorySummarizationFailedEventId = VKDiagnosticOffsets.AI_Cognitive_Memory + 1;
    public const int MemoryPrunedEventId = VKDiagnosticOffsets.AI_Cognitive_Memory + 2;
    public const int MemorySummarizedEventId = VKDiagnosticOffsets.AI_Cognitive_Memory + 3;

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
