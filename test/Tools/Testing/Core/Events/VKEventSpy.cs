using System.Collections.Concurrent;
using VK.Blocks.Core;

namespace VK.Blocks.Testing;

/// <summary>
/// In-memory test spy for inspecting dispatched <see cref="IVKDomainEvent"/> instances in integration tests.
/// </summary>
public sealed class VKEventSpy : IVKEventDispatcher
{
    private readonly ConcurrentBag<IVKDomainEvent> _publishedEvents = [];

    /// <summary>
    /// Gets the list of published domain events.
    /// </summary>
    public IReadOnlyList<IVKDomainEvent> PublishedEvents => [.. _publishedEvents];

    /// <inheritdoc />
    public ValueTask DispatchAsync(IVKDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _publishedEvents.Add(domainEvent);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask DispatchAsync(IEnumerable<IVKDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var @event in domainEvents)
        {
            _publishedEvents.Add(@event);
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Clears the recorded events.
    /// </summary>
    public void Reset()
    {
        _publishedEvents.Clear();
    }

    /// <summary>
    /// Checks if an event of type <typeparamref name="TEvent"/> matching the predicate has been published.
    /// </summary>
    public bool HasPublished<TEvent>(Func<TEvent, bool>? predicate = null) where TEvent : IVKDomainEvent
    {
        return _publishedEvents
            .OfType<TEvent>()
            .Any(e => predicate is null || predicate(e));
    }
}
