namespace VK.Blocks.AI.Engram;

/// <summary>
/// Defines the available compression strategy types.
/// </summary>
public enum VKCompressionStrategyType
{
    /// <summary>
    /// Summarizes the text content using chat engine.
    /// </summary>
    LlmSummary = 0,

    /// <summary>
    /// Extracts structured user facts and preferences as JSON.
    /// </summary>
    KeyValueExtraction = 1,

    /// <summary>
    /// Performs incremental, hierarchical summarization.
    /// </summary>
    HierarchicalSummary = 2,

    /// <summary>
    /// Identifies topic boundaries and performs segment-wise summarization.
    /// </summary>
    TopicSegmentation = 3
}
