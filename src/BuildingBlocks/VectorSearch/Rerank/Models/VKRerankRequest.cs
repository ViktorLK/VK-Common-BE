using System.Collections.Generic;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Represents a request for reranking search results.
/// </summary>
public sealed record VKRerankRequest
{
    /// <summary>
    /// Gets the search query.
    /// </summary>
    public required string Query { get; init; }

    /// <summary>
    /// Gets the search candidates to be reranked.
    /// </summary>
    public required IReadOnlyList<VKSearchResult> Candidates { get; init; }

    /// <summary>
    /// Gets the maximum number of results to return after reranking.
    /// </summary>
    public required int TopN { get; init; }
}
