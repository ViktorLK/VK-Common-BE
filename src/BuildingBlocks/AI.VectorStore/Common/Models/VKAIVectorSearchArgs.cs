using System.Collections.Generic;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Arguments for vector search operations.
/// </summary>
public sealed record VKAIVectorSearchArgs
{
    /// <summary>
    /// Gets the target tenant ID for the search.
    /// </summary>
    public required string TenantId { get; init; }

    /// <summary>
    /// Gets the maximum number of results to return.
    /// </summary>
    public int Limit { get; init; } = 5;

    /// <summary>
    /// Gets the minimum similarity score (0.0 to 1.0).
    /// </summary>
    public float MinScore { get; init; } = 0.7f;

    /// <summary>
    /// Gets or sets additional metadata filters.
    /// </summary>
    public IDictionary<string, string> Filters { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// [Data Pipeline] Gets or sets the advanced metadata filter (expression tree).
    /// </summary>
    public VK.Blocks.AI.VectorStore.VectorStore.Models.VKMetadataFilter? AdvancedFilter { get; init; }
}
