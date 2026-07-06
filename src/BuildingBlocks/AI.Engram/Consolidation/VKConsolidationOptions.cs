using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options for the Consolidation stage.
/// </summary>
[VKFeature(typeof(VKAIEngramBlock))]
public sealed partial record VKConsolidationOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Consolidation stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum consolidation batch size.
    /// </summary>
    public int MaxBatchSize { get; init; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether to enable schema updates.
    /// </summary>
    public bool EnableSchemaUpdate { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable user profile merging.
    /// </summary>
    public bool EnableUserProfileMerge { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable relation graph merging.
    /// </summary>
    public bool EnableGraphMerge { get; init; } = true;

    /// <summary>
    /// Gets or sets the threshold for cosine similarity checks.
    /// </summary>
    public double SimilarityThreshold { get; init; } = 0.85;

    /// <summary>
    /// Gets or sets the threshold for dropping lower-scoring duplicate memories.
    /// </summary>
    public double DropLowerThreshold { get; init; } = 0.90;

    /// <summary>
    /// Gets or sets the conflict strategy when merging memory schemas.
    /// </summary>
    public VKConsolidationConflictStrategy ConflictStrategy { get; init; } = VKConsolidationConflictStrategy.OverwriteLatest;

    /// <summary>
    /// Gets or sets a value indicating whether to enable confidence propagation.
    /// </summary>
    public bool EnableConfidencePropagation { get; init; } = true;
}
