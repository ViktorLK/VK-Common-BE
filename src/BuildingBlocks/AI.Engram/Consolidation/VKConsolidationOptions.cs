using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options for the Consolidation stage.
/// </summary>
public sealed partial record VKConsolidationOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Consolidation stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether background automatic consolidation is enabled.
    /// </summary>
    public bool EnableAutomaticConsolidation { get; init; } = true;

    /// <summary>
    /// Gets or sets the interval in minutes for periodic background consolidation sweeps.
    /// </summary>
    public int AutomaticConsolidationIntervalMinutes { get; init; } = 5;

    /// <summary>
    /// Gets or sets the maximum consolidation batch size.
    /// </summary>
    public int MaxBatchSize { get; init; } = 10;

    /// <summary>
    /// Gets or sets the maximum allowed content length for a single memory item before sanitization drops it.
    /// </summary>
    public int MaxMemoryContentLength { get; init; } = 2000;

    /// <summary>
    /// Gets or sets the threshold for cosine similarity checks.
    /// </summary>
    [VKRequestOverride]
    public double SimilarityThreshold { get; init; } = 0.85;

    /// <summary>
    /// Gets or sets the similarity threshold above which two L3 memories are considered redundant.
    /// Defaults to 0.8.
    /// </summary>
    public float L3RedundancyThreshold { get; init; } = 0.8f;

    /// <summary>
    /// Gets or sets a value indicating whether to merge redundant L3 memories using LLM or simply retain the higher-scoring one.
    /// Defaults to true.
    /// </summary>
    [VKRequestOverride]
    public bool EnableRedundancyMerge { get; init; } = true;

    /// <summary>
    /// Gets or sets the threshold for dropping lower-scoring duplicate memories.
    /// </summary>
    [VKRequestOverride]
    public double DropLowerThreshold { get; init; } = 0.90;

    /// <summary>
    /// Gets or sets the conflict strategy when merging memory schemas.
    /// </summary>
    public VKConsolidationConflictStrategy ConflictStrategy { get; init; } = VKConsolidationConflictStrategy.OverwriteLatest;

    /// <summary>
    /// Gets or sets the TopK count when querying existing candidates for contradiction arbitration.
    /// </summary>
    public int ArbitrationTopK { get; init; } = 100;
}
