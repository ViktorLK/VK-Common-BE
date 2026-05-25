using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Aggregates all Retrieval configuration settings.
/// </summary>
public interface IVKRetrievalOptions :
    IVKAIProviderOptions,
    IVKAIGovernanceOptions,
    IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the default maximum number of results to retrieve.
    /// </summary>
    int? TopK { get; init; }

    /// <summary>
    /// Gets the default minimum relevance score threshold (0.0 to 1.0).
    /// </summary>
    double? MinScore { get; init; }
}
