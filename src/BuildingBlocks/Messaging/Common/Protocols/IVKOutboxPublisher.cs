using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Messaging;

/// <summary>
/// Defines the contract for publishing events and sending commands via the Outbox pattern.
/// </summary>
public interface IVKOutboxPublisher
{
    /// <summary>
    /// Enqueues an event to be published when the current transaction commits successfully.
    /// </summary>
    Task<VKResult> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IVKEvent;

    /// <summary>
    /// Enqueues a command to be sent when the current transaction commits successfully.
    /// </summary>
    Task<VKResult> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, IVKCommand;
}
