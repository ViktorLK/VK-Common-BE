using VK.Blocks.Core.Diagnostics;
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostic tokens for the Knowledge feature.
/// </summary>
public static class VKKnowledgeDiagnosticTokens
{
    // Logs (Event IDs)
    public const int KnowledgeInitializedEventId = VKDiagnosticOffsets.AI_Afferent_Knowledge + 1;
    public const int KnowledgeRetrievedEventId = VKDiagnosticOffsets.AI_Afferent_Knowledge + 2;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string RetrievalDuration = "vk.ai.Afferent.knowledge.retrieval_duration";
        public const string EntriesExtracted = "vk.ai.Afferent.knowledge.entries_extracted";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string TriggerType = "vk.ai.knowledge.trigger_type";
        public const string SearchStrategy = "vk.ai.knowledge.search_strategy";
    }
}
