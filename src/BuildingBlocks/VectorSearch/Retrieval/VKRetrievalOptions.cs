using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Options for the Search stage.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock))]
public sealed partial record VKRetrievalOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Search stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default search limit.
    /// </summary>
    public int DefaultTopK { get; init; } = 5;

    /// <summary>
    /// Gets or sets the fusion strategy to use when merging results in hybrid search.
    /// </summary>
    public VKFusionStrategy HybridFusionStrategy { get; init; } = VKFusionStrategy.RRF;

    /// <summary>
    /// Gets or sets the weights to use for WeightedScoreFusion.
    /// The first weight is for dense/vector results, and the second is for sparse/keyword results.
    /// </summary>
    public float[] HybridWeights { get; init; } = [0.5f, 0.5f];
}
