namespace VK.Blocks.AI;

/// <summary>
/// Defines retrieval-specific parameters that can be overridden at the request level.
/// </summary>
public interface IVKRetrievalOverrides :
    IVKAIProviderOverrides,
    IVKAIGovernanceOverrides
{
    /// <summary>
    /// Gets the number of results to retrieve.
    /// </summary>
    int? TopK { get; init; }

    /// <summary>
    /// Gets the minimum score threshold.
    /// </summary>
    float? MinScore { get; init; }

    /// <summary>
    /// Gets a value indicating whether to enable temporal weighting.
    /// </summary>
    bool? EnableTemporalWeighting { get; init; }

    /// <summary>
    /// Gets the decay rate for temporal weighting.
    /// </summary>
    double? DecayRate { get; init; }
}
