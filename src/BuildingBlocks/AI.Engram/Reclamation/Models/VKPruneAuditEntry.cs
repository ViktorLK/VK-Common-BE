namespace VK.Blocks.AI.Engram;

/// <summary>
/// Detailed audit entry for a memory pruning action decision.
/// </summary>
public sealed record VKPruneAuditEntry
{
    /// <summary>
    /// Gets the unique identifier of the pruned memory entry.
    /// </summary>
    public required VKMemoryId MemoryId { get; init; }

    /// <summary>
    /// Gets the pruning action assigned to the memory entry (e.g. Delete, Archive, Flag).
    /// </summary>
    public required VKPruneAction Action { get; init; }

    /// <summary>
    /// Gets the evaluated RetentionScore at the time of pruning.
    /// </summary>
    public required float RetentionScore { get; init; }

    /// <summary>
    /// Gets the threshold applied to determine pruning.
    /// </summary>
    public required float Threshold { get; init; }

    /// <summary>
    /// Gets the category of the pruned memory entry.
    /// </summary>
    public required VKMemoryCategory Category { get; init; }

    /// <summary>
    /// Gets a truncated summary of the memory entry content for audit tracking.
    /// </summary>
    public string? ContentSummary { get; init; }
}
