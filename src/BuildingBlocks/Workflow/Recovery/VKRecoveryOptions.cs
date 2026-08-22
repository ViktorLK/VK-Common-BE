using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Options for Workflow orphan recovery background scanner.
/// </summary>
public sealed partial record VKRecoveryOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets the batch size limit when querying orphan workflows per scan tick.
    /// Defaults to 50.
    /// </summary>
    public int OrphanBatchLimit { get; init; } = 50;

    /// <summary>
    /// Gets or sets whether external status probing is enabled during recovery.
    /// Defaults to true.
    /// </summary>
    public bool EnableExternalStatusProbing { get; init; } = true;
}
