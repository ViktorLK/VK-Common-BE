using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Routing.Internal;

/// <summary>
/// Default no-op implementation of <see cref="IVKAICapacityLease"/> that grants immediate unconstrained access.
/// </summary>
// [ARCH-05] [AP.01] [AP.03]
internal sealed class NoOpCapacityLease : IVKAICapacityLease
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoOpCapacityLease"/> class.
    /// </summary>
    /// <param name="connectionId">The connection identifier associated with the lease.</param>
    public NoOpCapacityLease(string connectionId = "")
    {
        ConnectionId = VKGuard.NotNull(connectionId);
    }

    /// <inheritdoc />
    public bool IsAcquired => true;

    /// <inheritdoc />
    public string ConnectionId { get; }

    /// <inheritdoc />
    public string? ReservationId => null;

    /// <inheritdoc />
    public TimeSpan? RetryAfter => null;

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
