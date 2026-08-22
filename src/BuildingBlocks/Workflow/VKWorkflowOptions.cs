using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Global configuration options for the Workflow building block.
/// Follows BB.05 and AP.04.
/// </summary>
public sealed partial record VKWorkflowOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Workflow engine is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default timeout threshold in seconds before an in-flight Workflow is considered an orphan.
    /// Defaults to 300 seconds (5 minutes).
    /// </summary>
    public int DefaultTimeoutThresholdSeconds { get; init; } = 300;

    /// <summary>
    /// Gets or sets the maximum retry attempts for compensation handlers before marking as CompensationFailed.
    /// Defaults to 3.
    /// </summary>
    public int MaxCompensationRetries { get; init; } = 3;

    /// <summary>
    /// Gets or sets the interval in seconds for the background recovery sweeper to scan for orphan Workflows.
    /// Defaults to 60 seconds.
    /// </summary>
    public int OrphanScanIntervalSeconds { get; init; } = 60;
}
