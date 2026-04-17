using System;
namespace VK.Blocks.Core.Domain;

/// <summary>
/// Shorthand base for aggregate roots with a <see cref="Guid"/> primary key.
/// </summary>
public abstract class AggregateRoot : AggregateRoot<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot"/> class.
    /// Used for ORM hydration (e.g. EF Core).
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot"/> class.
    /// </summary>
    /// <param name="id">The aggregate root's identifier.</param>
    protected AggregateRoot(Guid id) : base(id)
    {
    }
}
