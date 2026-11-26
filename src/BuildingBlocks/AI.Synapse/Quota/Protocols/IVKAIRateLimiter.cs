using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Manages request rates (RPM) and concurrency (active requests) limits for AI connections via capacity leases.
/// </summary>
public interface IVKAIRateLimiter
{
    /// <summary>
    /// Checks non-destructively whether the specified connection currently has available concurrency and rate capacity.
    /// </summary>
    /// <param name="connection">The AI connection to inspect.</param>
    /// <returns><see langword="true"/> if capacity appears available; otherwise, <see langword="false"/>.</returns>
    bool HasAvailableCapacity(VKAIConnection connection);

    /// <summary>
    /// Acquires a capacity lease for the specified connection, optionally bound to a reservation identifier.
    /// </summary>
    /// <param name="connection">The AI connection to acquire capacity for.</param>
    /// <param name="reservationId">An optional reservation identifier associated with the attempt.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="IVKAICapacityLease"/> whose <see cref="IVKAICapacityLease.IsAcquired"/> indicates success.</returns>
    ValueTask<IVKAICapacityLease> AcquireLeaseAsync(
        VKAIConnection connection,
        string? reservationId = null,
        CancellationToken cancellationToken = default);
}
