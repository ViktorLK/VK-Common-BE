using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Error constants for the Consolidation stage.
/// </summary>
public static class VKConsolidationErrors
{
    public static readonly VKError FactExtractionFailed = new("AI.Engram.Consolidation.FactExtractionFailed", "Fact extraction strategy failed to process the content.");
    public static readonly VKError SchemaMergeError = new("AI.Engram.Consolidation.SchemaMergeError", "Schema merge failed.");
    public static readonly VKError MergeViaSummaryError = new("AI.Engram.Consolidation.MergeViaSummaryError", "LLM merge via summary failed.");
    public static readonly VKError PersistenceFailed = new("AI.Engram.Consolidation.PersistenceFailed", "Memory entry failed to persist after all retry attempts.");
    public static readonly VKError EmbeddingGenerationFailed = new("AI.Engram.Consolidation.EmbeddingGenerationFailed", "Failed to generate embedding for consolidated memory.");
    public static readonly VKError CrossSessionError = new("AI.Engram.Consolidation.CrossSessionError", "Cross-session memory consolidation failed.");
}
