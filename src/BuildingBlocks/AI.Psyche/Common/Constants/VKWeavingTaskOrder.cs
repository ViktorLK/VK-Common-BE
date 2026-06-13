namespace VK.Blocks.AI.Psyche.Weaving.Internal;

/// <summary>
/// Defines the order sequence for standard weaving tasks.
/// Tasks are executed in ascending order.
/// </summary>
public static class VKWeavingTaskOrder
{
    /// <summary>
    /// Defines the order for text and layout formatters (e.g., standardizing margins, adding basic separators).
    /// </summary>
    public const int Formatter = 300;

    /// <summary>
    /// Defines the order for truncation and pruning steps (e.g., token limit enforcement, trimming old history).
    /// </summary>
    public const int Truncate = 400;

    /// <summary>
    /// Defines the order for variable replacement steps (e.g., injecting actual persona names or specific runtime parameters).
    /// </summary>
    public const int Replacement = 800;

    /// <summary>
    /// Defines the step for resolving relative and absolute coordinate rules to final flat render orders.
    /// </summary>
    public const int CoordinateResolve = 900;

    /// <summary>
    /// Defines the final weaving assembly step where fragments are stitched together into the tapestry.
    /// </summary>
    public const int Weaving = 1000;

    /// <summary>
    /// A reserved order indicating the absolute final step in the pipeline.
    /// </summary>
    public const int Last = int.MaxValue;
}
