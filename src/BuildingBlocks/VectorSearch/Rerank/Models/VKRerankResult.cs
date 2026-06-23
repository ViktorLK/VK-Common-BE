namespace VK.Blocks.VectorSearch;

/// <summary>
/// Represents the result of a reranking operation.
/// </summary>
public sealed record VKRerankResult
{
    /// <summary>
    /// Gets the original search result before reranking.
    /// </summary>
    public required VKSearchResult Original { get; init; }

    /// <summary>
    /// Gets the new relevance score after reranking.
    /// </summary>
    public required double NewScore { get; init; }

    /// <summary>
    /// Gets the new 1-based rank after reranking.
    /// </summary>
    public required int NewRank { get; init; }
}
