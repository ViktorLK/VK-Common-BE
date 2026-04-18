using System;
namespace VK.Blocks.Core.Domain;

/// <summary>
/// Shorthand base for entities with a <see cref="Guid"/> primary key.
/// </summary>
public abstract class Entity : Entity<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class.
    /// Used for ORM hydration (e.g. EF Core).
    /// </summary>
    protected Entity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class.
    /// </summary>
    /// <param name="id">The entity's unique identifier.</param>
    protected Entity(Guid id) : base(id)
    {
    }
}
