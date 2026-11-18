using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents the routing decision containing ordered candidate connections and failover policy.
/// </summary>
// [AP.01]
public sealed record VKAIRouteDecision
{
    /// <summary>
    /// Gets the ordered list of candidate connections.
    /// </summary>
    public required IReadOnlyList<VKAIRouteCandidate> Candidates { get; init; }

    /// <summary>
    /// Gets the maximum number of failover attempts permitted.
    /// </summary>
    public int MaxFailoverAttempts { get; init; }

    /// <summary>
    /// Gets a value indicating whether failover across different providers is allowed.
    /// </summary>
    public bool AllowCrossProviderFailover { get; init; }

    /// <summary>
    /// Gets the optional routing profile name used to produce this decision.
    /// </summary>
    public string? ProfileName { get; init; }

    /// <summary>
    /// Gets the optional per-failure-kind map indicating whether failover is permitted.
    /// </summary>
    public IReadOnlyDictionary<VKAIFailureKind, bool>? SwitchableFailures { get; init; }
}
