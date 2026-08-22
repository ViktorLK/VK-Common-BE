using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Messaging;

/// <summary>
/// Defines the contract for handling events.
/// </summary>
public interface IVKEventHandler<in TEvent>
    where TEvent : class, IVKEvent
{
    /// <summary>
    /// Handles the event.
    /// </summary>
    Task HandleAsync(TEvent @event, VKMessageEnvelope envelope, CancellationToken cancellationToken = default);
}
