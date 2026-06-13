namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Defines Knowledge Sourcing settings that can be overridden at the request level.
/// </summary>
public interface IVKKnowledgeSourcingOverrides
{
    /// <summary>
    /// Gets the default token budget limit for injected corpus fragments.
    /// </summary>
    int? DefaultTokenBudget { get; init; }

    /// <summary>
    /// Gets the default maximum number of candidates to recall from vector store.
    /// </summary>
    int? DefaultTopK { get; init; }

    /// <summary>
    /// Gets the minimum similarity score threshold required for recalled candidates.
    /// </summary>
    double? DefaultMinScore { get; init; }
}
