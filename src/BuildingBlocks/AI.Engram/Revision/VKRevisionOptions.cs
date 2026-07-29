using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options for the Revision stage.
/// </summary>

public sealed partial record VKRevisionOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Revision stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum modifications permitted per hour per memory entry.
    /// </summary>
    public int MaxUpdatesPerHourPerEntry { get; init; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether to keep the previous version in metadata history.
    /// </summary>
    public bool KeepPreviousVersion { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether revising a memory entry flags dependent Synopsis summaries as stale.
    /// </summary>
    public bool EnableSynopsisCascadeInvalidation { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum history depth preserved in memory entry metadata for rollback support.
    /// </summary>
    public int MaxVersionHistoryDepth { get; init; } = 10;
}
