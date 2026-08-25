namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines how a knowledge entry is triggered.
/// </summary>
public enum VKKnowledgeTriggerType : byte
{
    /// <summary>
    /// Always active.
    /// </summary>
    Constant = 0,

    /// <summary>
    /// Triggered by keywords.
    /// </summary>
    Keyword = 1
}
