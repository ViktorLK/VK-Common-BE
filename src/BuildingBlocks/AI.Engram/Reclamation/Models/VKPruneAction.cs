using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy for handling pruned memory entries.
/// </summary>
public enum VKPruneAction
{
    /// <summary>
    /// Soft or hard delete the memory entry.
    /// </summary>
    Delete = 0,

    /// <summary>
    /// Move the memory entry to long-term storage or cold archive.
    /// </summary>
    Archive = 1,

    /// <summary>
    /// Flag the memory entry for manual administrative review without immediate deletion.
    /// </summary>
    Flag = 2
}
