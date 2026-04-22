using System.Collections.Generic;

namespace VK.Blocks.Core;

/// <summary>
/// Base class for aggregate roots - domain entities that serve as the
/// consistency boundary for a cluster of related objects.
/// Aggregates accumulate domain events that are dispatched after persistence.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
public abstract class VKAggregateRoot<TId> : VKEntity<TId> where TId : notnull
{
    private readonly List<IVKDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the domain events raised by this aggregate since its last save.
    /// </summary>
    public IReadOnlyList<IVKDomainEvent> DomainEvents => _domainEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKAggregateRoot{TId}"/> class.
    /// Used for ORM hydration (e.g. EF Core).
    /// </summary>
    protected VKAggregateRoot()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKAggregateRoot{TId}"/> class.
    /// </summary>
    /// <param name="id">The aggregate's identifier.</param>
    protected VKAggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// Raises and enqueues a domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void RaiseDomainEvent(IVKDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>
    /// Clears all accumulated domain events (called after dispatch).
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
