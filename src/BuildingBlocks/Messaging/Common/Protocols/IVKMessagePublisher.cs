using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Messaging;

/// <summary>
/// Publisher interface for sending events (1-to-many pub/sub).
/// </summary>
public interface IVKMessagePublisher
{
    /// <summary>
    /// Publishes an event to the messaging topology.
    /// </summary>
    Task<VKResult> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IVKEvent;
}
