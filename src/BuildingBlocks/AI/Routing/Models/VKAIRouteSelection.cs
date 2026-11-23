using System;

namespace VK.Blocks.AI;

/// <summary>
/// Represents a selected AI route candidate paired with its capacity lease and routing decision context.
/// </summary>
// [AP.01]
public sealed class VKAIRouteSelection : IDisposable
{
    /// <summary>
    /// Gets the selected candidate connection.
    /// </summary>
    public required VKAIRouteCandidate Candidate { get; init; }

    /// <summary>
    /// Gets the capacity lease held for the selected candidate.
    /// </summary>
    public required IVKAICapacityLease Lease { get; init; }

    /// <summary>
    /// Gets the routing decision that produced this selection.
    /// </summary>
    public required VKAIRouteDecision Decision { get; init; }

    /// <summary>
    /// Gets the 1-based attempt number for this selection.
    /// </summary>
    public int AttemptNo { get; init; } = 1;

    /// <summary>
    /// Gets the maximum number of attempts permitted under the routing decision.
    /// </summary>
    public int MaxAttempts { get; init; } = 1;

    /// <summary>
    /// Gets the optional reservation identifier from the underlying capacity lease.
    /// </summary>
    public string? ReservationId => Lease.ReservationId;

    /// <inheritdoc />
    public void Dispose() => Lease.Dispose();
}
