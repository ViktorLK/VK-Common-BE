using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Options contract for the Reasoning feature.
/// </summary>
public interface IVKReasoningOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether to allow parallel execution of independent steps.
    /// </summary>
    bool AllowParallelism { get; }

    /// <summary>
    /// Gets the maximum depth for hierarchical reasoning.
    /// </summary>
    int MaxDepth { get; }
}
