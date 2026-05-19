namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Request-level overrides contract for the Reasoning feature.
/// </summary>
public interface IVKReasoningOverrides
{
    /// <summary>
    /// Gets a value indicating whether to allow parallel execution of independent steps.
    /// </summary>
    bool? AllowParallelism { get; init; }

    /// <summary>
    /// Gets the maximum depth for hierarchical reasoning.
    /// </summary>
    int? MaxDepth { get; init; }
}
