namespace VK.Blocks.VectorSearch;

/// <summary>
/// Represents the result of a sparse search.
/// </summary>
public sealed record VKSparseSearchResult
{
    /// <summary>
    /// Gets the document chunk.
    /// </summary>
    public required VKDocumentChunk Chunk { get; init; }

    /// <summary>
    /// Gets the keyword/sparse search score.
    /// </summary>
    public required float Score { get; init; }
}
