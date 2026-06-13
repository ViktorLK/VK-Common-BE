using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Aggregates all static Knowledge Sourcing configuration options.
/// </summary>
public interface IVKKnowledgeSourcingOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the default token budget limit for injected corpus fragments.
    /// </summary>
    int? DefaultTokenBudget { get; }

    /// <summary>
    /// Gets the default maximum number of candidates to recall from vector store.
    /// </summary>
    int DefaultTopK { get; }

    /// <summary>
    /// Gets the minimum similarity score threshold required for recalled candidates.
    /// </summary>
    double? DefaultMinScore { get; }
}
