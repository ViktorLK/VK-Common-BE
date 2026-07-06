namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy options when conflicts occur during memory schema updates.
/// </summary>
public enum VKConsolidationConflictStrategy
{
    /// <summary>
    /// Always overwrite with the newest extracted fact.
    /// </summary>
    OverwriteLatest,

    /// <summary>
    /// Require multiple confirmations before changing the fact.
    /// </summary>
    RequireMultipleConfirmations,

    /// <summary>
    /// Keep both facts and mark them as conflicting.
    /// </summary>
    Coexist
}
