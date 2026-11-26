using System.Collections.Generic;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Named routing profile defining candidate connections and routing strategy.
/// Follows AP.01 (sealed record).
/// </summary>
public sealed record VKAIProfile
{
    public required string Name { get; init; }
    public required IReadOnlyList<string> CandidateConnectionIds { get; init; }
    public VKAIRoutingStrategy Strategy { get; init; } = VKAIRoutingStrategy.Priority;
    public bool AllowCrossProviderFailover { get; init; } = true;
    public string? FallbackProfileName { get; init; }
}
