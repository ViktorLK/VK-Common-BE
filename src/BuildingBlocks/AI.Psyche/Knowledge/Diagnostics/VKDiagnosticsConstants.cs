using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche.Knowledge;

/// <summary>
/// Public diagnostic tokens for the Knowledge feature.
/// </summary>
internal static class VKDiagnosticsConstants
{
    // Logs (Event IDs)
    public static class Logs
    {
        public const int KnowledgeInitialized = VKDiagnosticOffsets.AI_Psyche_Knowledge + 1;
        public const int FactArchived = VKDiagnosticOffsets.AI_Psyche_Knowledge + 2;
        public const int LedgerNotImplemented = VKDiagnosticOffsets.AI_Psyche_Knowledge + 3;
    }

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string RetrievalDuration = "vk.ai.psyche.knowledge.retrieval_duration";
        public const string EntriesExtracted = "vk.ai.psyche.knowledge.entries_extracted";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string TriggerType = "vk.ai.knowledge.trigger_type";
        public const string SearchStrategy = "vk.ai.knowledge.search_strategy";
    }
}
