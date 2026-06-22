namespace VK.Blocks.VectorSearch;

/// <summary>
/// Represents a candidate item with its score or rank to be used in fusion algorithms.
/// </summary>
public sealed record VKFusionCandidate
{
    /// <summary>
    /// Gets the document chunk.
    /// </summary>
    public required VKDocumentChunk Chunk { get; init; }

    /// <summary>
    /// Gets the score or weight from a retrieval search.
    /// </summary>
    public required float Score { get; init; }

    /// <summary>
    /// Gets the 1-based rank from the retrieval search.
    /// </summary>
    public required int Rank { get; init; }
}
