namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Specifies the origin or provenance of a knowledge entry in the corpus.
/// </summary>
public enum VKKnowledgeProvenance
{
    /// <summary>
    /// Manually created or curated knowledge.
    /// </summary>
    Manual = 0,

    /// <summary>
    /// Extracted automatically by AI from dialogue or user intent.
    /// </summary>
    AIExtracted = 1,

    /// <summary>
    /// Imported from an external document or batch ingestion source.
    /// </summary>
    Imported = 2
}
