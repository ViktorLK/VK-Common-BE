using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Options for the Gathering feature of AI.Corpus.
/// </summary>

public sealed partial record VKGatheringOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the default token budget limit for injected corpus fragments.
    /// </summary>
    [VKRequestOverride]
    public int? DefaultTokenBudget { get; init; } = 2048;

    /// <summary>
    /// Gets the default maximum number of candidates to recall from vector store.
    /// </summary>
    [VKRequestOverride]
    public int DefaultTopK { get; init; } = 20;

    /// <summary>
    /// Gets the minimum similarity score threshold required for recalled candidates.
    /// </summary>
    [VKRequestOverride]
    public double? DefaultMinScore { get; init; } = 0.7;
}
