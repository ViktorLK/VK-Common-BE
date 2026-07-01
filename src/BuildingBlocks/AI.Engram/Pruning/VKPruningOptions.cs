using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options for the Pruning stage.
/// </summary>
[VKFeature(typeof(VKAIEngramBlock))]
public sealed partial record VKPruningOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Pruning stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the threshold below which engrams will be pruned.
    /// </summary>
    public double Threshold { get; init; } = 0.3;

    /// <summary>
    /// Gets or sets the L1 memory pruning threshold.
    /// </summary>
    public double L1Threshold { get; init; } = 0.2;

    /// <summary>
    /// Gets or sets the L2 memory pruning threshold.
    /// </summary>
    public double L2Threshold { get; init; } = 0.35;

    /// <summary>
    /// Gets or sets the L3 memory pruning threshold.
    /// </summary>
    public double L3Threshold { get; init; } = 0.45;

    /// <summary>
    /// Gets or sets a value indicating whether to prioritize pruning of inferred (low-confidence) memories.
    /// </summary>
    public bool InferredPriorityPruning { get; init; } = true;

    /// <summary>
    /// Gets or sets the multiplier applied to the pruning threshold for inferred memories.
    /// </summary>
    public double InferredThresholdMultiplier { get; init; } = 1.2;

    /// <summary>
    /// Gets or sets a value indicating whether to run in Dry Run mode (logs decisions without deleting data).
    /// </summary>
    public bool DryRunMode { get; init; } = false;

    /// <summary>
    /// Gets or sets the soft delete grace period in days before physical data removal.
    /// </summary>
    public int SoftDeleteGracePeriodDays { get; init; } = 7;
}
