using VK.Blocks.Core.Events;

namespace VK.Blocks.Core.Primitives;

/// <summary>
/// Base class for all domain entities with a strongly-typed identity.
/// Implements value equality by entity identity.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier. Must be non-null.</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    #region Constructors

    // For ORM hydration (e.g. EF Core)
    protected Entity() { }

    protected Entity(TId id) => Id = id;

    #endregion

    #region Properties

    /// <summary>Gets the entity's unique identifier.</summary>
    public TId Id { get; protected set; } = default!;

    #endregion

    #region Equality

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        if (GetType() != other.GetType())
            return false;
        return Id.Equals(other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);

    #endregion
}

/// <summary>
/// Shorthand base for entities with a <see cref="Guid"/> primary key.
/// </summary>
public abstract class Entity : Entity<Guid>
{
    protected Entity() { }
    protected Entity(Guid id) : base(id) { }
}
