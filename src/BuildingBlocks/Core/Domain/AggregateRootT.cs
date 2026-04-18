using System.Collections.Generic;
using VK.Blocks.Core.Domain.Events;

namespace VK.Blocks.Core.Domain;

/// <summary>
/// Base class for aggregate roots  Edomain entities that serve as the
/// consistency boundary for a cluster of related objects.
/// Aggregates accumulate domain events that are dispatched after persistence.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the domain events raised by this aggregate since its last save.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// Used for ORM hydration (e.g. EF Core).
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TId}"/> class.
    /// </summary>
    /// <param name="id">The aggregate's identifier.</param>
    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// Raises and enqueues a domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>
    /// Clears all accumulated domain events (called after dispatch).
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
