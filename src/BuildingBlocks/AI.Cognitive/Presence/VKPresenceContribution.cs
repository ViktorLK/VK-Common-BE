using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents environmental, relational, or behavioral metadata contributed to the presence tapestry.
/// Follows AP.01 (sealed record with required properties) and AP.03.
/// </summary>
public sealed record VKPresenceContribution
{
    /// <summary>
    /// Gets the contributed prompt overlay segment (e.g., formatted markdown points).
    /// </summary>
    public required string PromptSegment { get; init; }

    /// <summary>
    /// Gets the prioritized claims contributed by this component for conflict resolution.
    /// </summary>
    public IReadOnlyDictionary<string, VKContributionValue> Claims { get; init; } = new Dictionary<string, VKContributionValue>();

    /// <summary>
    /// Gets general telemetry or tracking tags.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}
