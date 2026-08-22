using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Messaging;

/// <summary>
/// An in-memory implementation of message publisher and sender for testing purposes.
/// </summary>
internal sealed class InMemoryMessageBus : IVKMessagePublisher, IVKCommandSender
{
    private readonly ConcurrentQueue<IVKEvent> _publishedEvents = new();
    private readonly ConcurrentQueue<IVKCommand> _sentCommands = new();

    public IReadOnlyCollection<IVKEvent> PublishedEvents => this._publishedEvents;
    public IReadOnlyCollection<IVKCommand> SentCommands => this._sentCommands;

    public Task<VKResult> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IVKEvent
    {
        VKGuard.NotNull(@event);
        this._publishedEvents.Enqueue(@event);
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, IVKCommand
    {
        VKGuard.NotNull(command);
        this._sentCommands.Enqueue(command);
        return Task.FromResult(VKResult.Success());
    }

    public void Clear()
    {
        this._publishedEvents.Clear();
        this._sentCommands.Clear();
    }
}
