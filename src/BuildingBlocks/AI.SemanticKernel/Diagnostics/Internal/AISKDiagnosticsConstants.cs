namespace VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;

/// <summary>
/// Constants for the Semantic Kernel building block diagnostics.
/// </summary>
internal static class AISKDiagnosticsConstants
{
    /// <summary>
    /// The name of the AISK service.
    /// </summary>
    public const string ServiceName = "AI.AISK";

    /// <summary>
    /// Tag keys for Semantic Kernel diagnostics.
    /// </summary>
    internal static class Tags
    {
        public const string ModelId = "vk.ai.sk.model_id";
        public const string ServiceType = "vk.ai.sk.service_type";
        public const string Intent = "vk.ai.sk.intent";
        public const string TokenType = "vk.ai.chat.token_type";
    }

    /// <summary>
    /// Metric names for Semantic Kernel diagnostics.
    /// </summary>
    internal static class Metrics
    {
        public const string ChatRequestDuration = "vk.ai.chat.request_duration";
        public const string ChatTokenUsage = "vk.ai.chat.tokens_total";
        public const string EmbeddingGenerationDuration = "vk.ai.embedding.generation_duration";
        public const string EmbeddingItemsCount = "vk.ai.embedding.items_total";
    }
}
