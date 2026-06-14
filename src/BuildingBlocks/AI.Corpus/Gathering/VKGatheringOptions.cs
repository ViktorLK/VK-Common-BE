using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Options for the Gathering feature of AI.Corpus.
/// </summary>
[VKFeature(typeof(VKAICorpusBlock), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKGatheringOptions : IVKGatheringOptions
{
    /// <summary>
    /// Gets the default token budget limit for injected corpus fragments.
    /// </summary>
    public int? DefaultTokenBudget { get; init; } = 2048;

    /// <summary>
    /// Gets the default maximum number of candidates to recall from vector store.
    /// </summary>
    public int DefaultTopK { get; init; } = 20;

    /// <summary>
    /// Gets the minimum similarity score threshold required for recalled candidates.
    /// </summary>
    public double? DefaultMinScore { get; init; } = 0.7;
}
