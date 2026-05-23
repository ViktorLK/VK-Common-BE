namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Public diagnostic tokens for the Knowledge feature.
/// </summary>
public static class VKKnowledgeDiagnosticTokens
{
    // Logs (Event IDs)
    public const int KnowledgeInitializedEventId = 200;
    public const int KnowledgeRetrievedEventId = 201;

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string RetrievalDuration = "vk.ai.cognitive.knowledge.retrieval_duration";
        public const string EntriesExtracted = "vk.ai.cognitive.knowledge.entries_extracted";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string TriggerType = "vk.ai.knowledge.trigger_type";
        public const string SearchStrategy = "vk.ai.knowledge.search_strategy";
    }
}
