using VK.Blocks.Core.Diagnostics;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Public diagnostic tokens for the Knowledge feature.
/// Follows BB.04 and OR.01.
/// </summary>
public static class VKKnowledgeDiagnosticsConstants
{
    // Logs (Event IDs: 74000 - 74999)
    public static class Logs
    {
        public const int KnowledgeInitialized = VKDiagnosticOffsets.AI_Psyche_Knowledge + 1;
        public const int FactArchived = VKDiagnosticOffsets.AI_Psyche_Knowledge + 2;
        public const int LedgerNotImplemented = VKDiagnosticOffsets.AI_Psyche_Knowledge + 3;
        public const int KnowledgeMatched = VKDiagnosticOffsets.AI_Psyche_Knowledge + 4;
        public const int KnowledgeEvaluationCompleted = VKDiagnosticOffsets.AI_Psyche_Knowledge + 5;
    }

    // Metrics (Meter/Counter/Histogram Names)
    public static class Metrics
    {
        public const string RetrievalDuration = "vk.ai.psyche.knowledge.retrieval_duration";
        public const string EntriesMatched = "vk.ai.psyche.knowledge.entries_matched";
        public const string ConstantEntriesCount = "vk.ai.psyche.knowledge.constant_entries_count";
        public const string ConditionalEntriesMatched = "vk.ai.psyche.knowledge.conditional_entries_matched";
    }

    // Tags (Telemetry Dimensions)
    public static class Tags
    {
        public const string StageName = "ai.psyche.stage";
        public const string SearchStrategy = "vk.ai.knowledge.search_strategy";
        public const string TriggerType = "vk.ai.knowledge.trigger_type";
        public const string MatchedCount = "gen_ai.knowledge.matched_count";
    }
}
