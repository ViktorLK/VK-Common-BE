namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines the formula strategy used for memory decay calculations.
/// </summary>
public enum VKDecayMode
{
    /// <summary>
    /// Exponential decay model based on Ebbinghaus forgetting curve: exp(-elapsed / halfLife).
    /// </summary>
    Exponential = 0,

    /// <summary>
    /// Linear decay model: max(0, 1 - elapsed / halfLife).
    /// </summary>
    Linear = 1,

    /// <summary>
    /// Stepped decay model: drops by 50% for each elapsed half-life period.
    /// </summary>
    Stepped = 2
}
