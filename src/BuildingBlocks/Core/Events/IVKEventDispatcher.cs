using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Defines the contract for dispatching domain events to their handlers.
/// </summary>
public interface IVKEventDispatcher
{
    /// <summary>
    /// Dispatches a single domain event to all registered handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event to dispatch.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask DispatchAsync(IVKDomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches multiple domain events sequentially to all registered handlers.
    /// </summary>
    /// <param name="domainEvents">The domain events to dispatch.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask DispatchAsync(IEnumerable<IVKDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
