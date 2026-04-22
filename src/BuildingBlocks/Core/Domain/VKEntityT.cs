using System;

namespace VK.Blocks.Core;

/// <summary>
/// Base class for all domain entities with a strongly-typed identity.
/// Implements value equality by entity identity.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier. Must be non-null.</typeparam>
public abstract class VKEntity<TId> where TId : notnull
{
    /// <summary>
    /// Gets the entity's unique identifier.
    /// </summary>
    /// <remarks>
    /// Initialized with default! to support ORM parameterless constructor requirements (Rule 12 Exception).
    /// Guaranteed to be non-null after hydration or explicit constructor initialization.
    /// </remarks>
    public TId Id { get; init; } = default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKEntity{TId}"/> class.
    /// Used for ORM hydration (e.g. EF Core).
    /// </summary>
    protected VKEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKEntity{TId}"/> class.
    /// </summary>
    /// <param name="id">The entity's unique identifier.</param>
    protected VKEntity(TId id) => Id = id;

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not VKEntity<TId> other)
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
    /// Compares two <see cref="VKEntity{TId}"/> instances for equality.
    /// </summary>
    /// <param name="left">The left entity.</param>
    /// <param name="right">The right entity.</param>
    /// <returns><c>true</c> if they are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(VKEntity<TId>? left, VKEntity<TId>? right) => Equals(left, right);
    public static bool operator !=(VKEntity<TId>? left, VKEntity<TId>? right) => !Equals(left, right);
}
