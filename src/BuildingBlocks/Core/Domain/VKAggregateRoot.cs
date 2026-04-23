using System;

namespace VK.Blocks.Core;

/// <summary>
/// Shorthand base for aggregate roots with a <see cref="Guid"/> primary key.
/// </summary>
public abstract class VKAggregateRoot : VKAggregateRoot<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKAggregateRoot"/> class.
    /// Used for ORM hydration (e.g. EF Core).
    /// </summary>
    protected VKAggregateRoot()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKAggregateRoot"/> class.
    /// </summary>
    /// <param name="id">The aggregate root's identifier.</param>
    protected VKAggregateRoot(Guid id) : base(id)
    {
    }
}
