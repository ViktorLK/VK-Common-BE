namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines weaving settings that can be overridden at the request level.
/// </summary>
public interface IVKWeavingOverrides
{
    /// <summary>
    /// Gets the maximum token limit for the weaving process.
    /// </summary>
    int? MaxTokenLimit { get; init; }

    /// <summary>
    /// Gets whether to strip think tags (e.g. &lt;think&gt;) from the prompt.
    /// </summary>
    bool? StripThinkTags { get; init; }

    /// <summary>
    /// Gets whether to enable semantic pruning when token limits are exceeded.
    /// </summary>
    bool? EnableSemanticPruning { get; init; }
}
