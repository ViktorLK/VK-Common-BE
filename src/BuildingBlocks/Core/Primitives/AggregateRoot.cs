using VK.Blocks.Core.Events;

namespace VK.Blocks.Core.Primitives;

/// <summary>
/// Base class for aggregate roots  Edomain entities that serve as the
/// consistency boundary for a cluster of related objects.
/// Aggregates accumulate domain events that are dispatched after persistence.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    #region Fields

    private readonly List<IDomainEvent> _domainEvents = [];

    #endregion

    #region Constructors

    protected AggregateRoot() { }

    protected AggregateRoot(TId id) : base(id) { }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the domain events raised by this aggregate since its last save.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    #endregion

    #region Methods

    /// <summary>Raises and enqueues a domain event.</summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>Clears all accumulated domain events (called after dispatch).</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    #endregion
}

/// <summary>
/// Shorthand base for aggregate roots with a <see cref="Guid"/> primary key.
/// </summary>
public abstract class AggregateRoot : AggregateRoot<Guid>
{
    protected AggregateRoot() { }
    protected AggregateRoot(Guid id) : base(id) { }
}
