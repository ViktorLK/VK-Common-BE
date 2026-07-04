using System;

namespace VK.Blocks.Core;

/// <summary>
/// Shorthand base for entities with a <see cref="Guid"/> primary key.
/// </summary>
public abstract class VKEntity : VKEntity<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKEntity"/> class.
    /// Used for ORM hydration (e.g. EF Core).
    /// </summary>
    protected VKEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKEntity"/> class.
    /// </summary>
    /// <param name="id">The entity's unique identifier.</param>
    protected VKEntity(Guid id) : base(id)
    {
    }
}
