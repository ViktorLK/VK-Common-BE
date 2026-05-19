namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents advanced prompt-weaving, priority, and recursion limit configurations
/// associated with a knowledge entry.
/// Follows AP.01 (Sealed Record) and AP.03.
/// </summary>
// // [AP.01] Sealed record for immutability
public sealed record VKKnowledgeWeavingRules
{
    /// <summary>
    /// Gets the target prompt template position where this entry should be woven.
    /// Defaults to <see cref="VKKnowledgePositions.BeforeDefs"/>.
    /// </summary>
    public VKKnowledgePositions Position { get; init; } = VKKnowledgePositions.BeforeDefs;

    /// <summary>
    /// Gets the rendering priority order of the entry when triggered.
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Gets the weight of this entry for conflict resolution.
    /// Higher weight entries take precedence.
    /// </summary>
    public int Weight { get; init; } = 0;

    /// <summary>
    /// Gets the prompt insertion depth index.
    /// Determines the woven proximity relative to prompt borders. Defaults to 0.
    /// </summary>
    public int Depth { get; init; } = 0;

    /// <summary>
    /// Gets a value indicating whether this entry cannot be triggered recursively (Non-recursive / Disable recursion).
    /// If true, it can only be triggered on the initial context (depth = 0).
    /// </summary>
    public bool DisableRecursion { get; init; } = false;

    /// <summary>
    /// Gets a value indicating whether this entry prevents further recursive triggers (Prevent further recursion).
    /// If true, its content will not be enqueued to match other entries.
    /// </summary>
    public bool PreventFurtherRecursion { get; init; } = false;

    /// <summary>
    /// Gets a value indicating whether this entry can only be triggered recursively (Delay to recursion / Recursive only).
    /// If true, it will be skipped on the initial context scan (depth = 0).
    /// </summary>
    public bool RecursiveOnly { get; init; } = false;

    /// <summary>
    /// Gets the maximum allowed recursive match jump depth for this entry.
    /// </summary>
    public int MaxRecursionLevel { get; init; } = 0;

    /// <summary>
    /// Gets a static default instance of <see cref="VKKnowledgeWeavingRules"/>.
    /// </summary>
    public static VKKnowledgeWeavingRules Default { get; } = new();
}
