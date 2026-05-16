namespace VK.Blocks.AI;

/// <summary>
/// Defines retrieval-specific parameters that can be overridden at the request level.
/// </summary>
public interface IVKRetrievalOverrides :
    IVKAIProviderOverrides,
    IVKAIResilienceOverrides,
    IVKAIQuotaOverrides
{
    /// <summary>
    /// Gets the number of results to retrieve.
    /// </summary>
    int? TopK { get; init; }

    /// <summary>
    /// Gets the minimum similarity threshold.
    /// </summary>
    float? MinSimilarity { get; init; }
}
