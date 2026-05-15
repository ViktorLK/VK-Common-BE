namespace VK.Blocks.AI.VectorStore.Diagnostics;

/// <summary>
/// Constants for AI Vector Store diagnostics.
/// Follows OR.01 requirement for centralized semantic tokens.
/// </summary>
public static class VKAIVectorStoreDiagnosticsConstants
{
    public static class Metrics
    {
        public const string SearchDuration = "vk_ai_vector_search_duration";
        public const string RecallHits = "vk_ai_vector_recall_hits";
    }

    public static class Tags
    {
        public const string ModelId = "model_id";
    }
}
