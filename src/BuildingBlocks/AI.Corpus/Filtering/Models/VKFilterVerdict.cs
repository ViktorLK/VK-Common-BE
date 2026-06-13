namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Represents the evaluation verdict of a corpus entry filter.
/// </summary>
public enum VKFilterVerdict
{
    /// <summary>
    /// The entry is rejected by the filter.
    /// </summary>
    Reject = 0,

    /// <summary>
    /// The entry is permitted to continue to subsequent filters.
    /// </summary>
    Keep = 1,

    /// <summary>
    /// The entry is forced to be kept, bypassing subsequent filters.
    /// </summary>
    ForceKeep = 2
}
