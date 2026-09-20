using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the order sequence for standard weaving tasks.
/// Tasks are executed in ascending order.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Static constants class without logic.")]
public static class VKWeavingTaskOrder
{
    /// <summary>
    /// Defines the order for variable and placeholder replacement steps (injecting dynamic values into templates).
    /// Runs before truncation so token budget evaluation reflects actual rendered content.
    /// </summary>
    public const int Replacement = 300;

    /// <summary>
    /// Defines the order for truncation and pruning steps (e.g., token limit enforcement, trimming old history).
    /// </summary>
    public const int Truncate = 400;

    /// <summary>
    /// Defines the final weaving assembly step where fragments are stitched together into the tapestry.
    /// </summary>
    public const int Weaving = 1000;

    /// <summary>
    /// A reserved order indicating the absolute final step in the pipeline.
    /// </summary>
    public const int Last = int.MaxValue;
}
