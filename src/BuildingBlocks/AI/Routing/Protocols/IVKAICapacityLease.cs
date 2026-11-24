using System;

namespace VK.Blocks.AI;

/// <summary>
/// Represents an acquired capacity lease for a routed AI provider connection.
/// </summary>
// [ARCH-02]
public interface IVKAICapacityLease : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the capacity lease was acquired.
    /// </summary>
    bool IsAcquired { get; }

    /// <summary>
    /// Gets the connection identifier associated with the lease.
    /// </summary>
    string ConnectionId { get; }

    /// <summary>
    /// Gets the optional reservation identifier associated with the lease.
    /// </summary>
    string? ReservationId { get; }

    /// <summary>
    /// Gets the optional retry-after duration if capacity was not acquired.
    /// </summary>
    TimeSpan? RetryAfter { get; }
}
