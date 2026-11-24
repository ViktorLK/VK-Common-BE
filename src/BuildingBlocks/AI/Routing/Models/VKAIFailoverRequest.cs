using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a request to select a subsequent failover AI connection candidate after a failed attempt.
/// </summary>
// [AP.01]
public sealed record VKAIFailoverRequest
{
    /// <summary>
    /// Gets the original routing request.
    /// </summary>
    public required VKAIRouteRequest OriginalRequest { get; init; }

    /// <summary>
    /// Gets the routing decision established for the logical invocation.
    /// </summary>
    public required VKAIRouteDecision Decision { get; init; }

    /// <summary>
    /// Gets the connection identifiers that have already been tried or excluded.
    /// </summary>
    public required IReadOnlyCollection<string> ExcludedConnectionIds { get; init; }

    /// <summary>
    /// Gets the outcome of the most recent failed attempt.
    /// </summary>
    public required VKAIInvocationOutcome LastFailure { get; init; }

    /// <summary>
    /// Gets the 1-based attempt number for the requested failover attempt.
    /// </summary>
    public int NextAttemptNo { get; init; }
}
