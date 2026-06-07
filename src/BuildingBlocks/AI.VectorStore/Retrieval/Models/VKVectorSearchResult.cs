namespace VK.Blocks.AI.VectorStore.Retrieval.Models;

/// <summary>
/// Represents the result of a vector search.
/// </summary>
public sealed record VKVectorSearchResult
{
    /// <summary>
    /// Gets the document chunk.
    /// </summary>
    public required VKDocumentChunk Chunk { get; init; }

    /// <summary>
    /// Gets the relevance score.
    /// </summary>
    public required float Score { get; init; }
}
