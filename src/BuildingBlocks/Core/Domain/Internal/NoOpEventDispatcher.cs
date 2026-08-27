using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core.Domain.Internal;

/// <summary>
/// A default no-operation implementation of <see cref="IVKEventDispatcher"/>.
/// Safely ignores any dispatched domain events without allocating resources.
/// Follows [AP.01], [CS.03].
/// </summary>
internal sealed class NoOpEventDispatcher : IVKEventDispatcher // [AP.01]
{
    /// <inheritdoc />
    public ValueTask DispatchAsync(IVKDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask; // [CS.03]
    }

    /// <inheritdoc />
    public ValueTask DispatchAsync(IEnumerable<IVKDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask; // [CS.03]
    }
}
