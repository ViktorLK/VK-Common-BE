namespace VK.Blocks.Core.Primitives;

/// <summary>
/// Base class for all domain entities with a strongly-typed identity.
/// Implements value equality by entity identity.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier. Must be non-null.</typeparam>
public abstract class Entity<TId> where TId : notnull
{
    /// <summary>
    /// Gets the entity's unique identifier.
    /// </summary>
    public TId Id { get; init; } = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// Used for ORM hydration (e.g. EF Core).
    /// </summary>
    protected Entity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// </summary>
    /// <param name="id">The entity's unique identifier.</param>
    protected Entity(TId id) => Id = id;

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return Id.Equals(other.Id);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    /// <summary>
    /// Compares two <see cref="Entity{TId}"/> instances for equality.
    /// </summary>
    /// <param name="left">The left entity.</param>
    /// <param name="right">The right entity.</param>
    /// <returns><c>true</c> if they are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    /// <summary>
    /// Compares two <see cref="Entity{TId}"/> instances for inequality.
    /// </summary>
    /// <param name="left">The left entity.</param>
    /// <param name="right">The right entity.</param>
    /// <returns><c>true</c> if they are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
