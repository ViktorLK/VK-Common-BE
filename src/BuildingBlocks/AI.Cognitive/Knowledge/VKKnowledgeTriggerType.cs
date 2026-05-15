namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines how a knowledge entry is triggered.
/// </summary>
public enum VKKnowledgeTriggerType
{
    /// <summary>
    /// Triggered by keywords.
    /// </summary>
    Keyword,

    /// <summary>
    /// Triggered by semantic similarity.
    /// </summary>
    Semantic,

    /// <summary>
    /// Always active.
    /// </summary>
    Constant
}
