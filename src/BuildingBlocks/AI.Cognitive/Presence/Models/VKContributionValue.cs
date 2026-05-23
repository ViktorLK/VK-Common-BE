using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents a claim value with an associated priority score for resolving conflicts.
/// Follows AP.01 (sealed record with required properties) and AP.03.
/// </summary>
public sealed record VKContributionValue
{
    /// <summary>
    /// Gets the claim value.
    /// </summary>
    public required object Value { get; init; }

    /// <summary>
    /// Gets the priority score (higher values override lower values during conflict resolution).
    /// </summary>
    public required int Priority { get; init; }
}
