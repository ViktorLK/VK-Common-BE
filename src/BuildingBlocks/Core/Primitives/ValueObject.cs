namespace VK.Blocks.Core.Primitives;

/// <summary>
/// Base class for value objects — immutable, structurally-equal domain concepts
/// with no identity of their own (e.g., Money, Address, DateRange).
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other)
        {
            return false;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public bool Equals(ValueObject? other) =>
        other is not null && Equals((object)other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(0, (hash, component) => HashCode.Combine(hash, component));

    /// <summary>
    /// Compares two <see cref="ValueObject"/> instances for equality.
    /// </summary>
    /// <param name="left">The left value object.</param>
    /// <param name="right">The right value object.</param>
    /// <returns><c>true</c> if they are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    /// <summary>
    /// Compares two <see cref="ValueObject"/> instances for inequality.
    /// </summary>
    /// <param name="left">The left value object.</param>
    /// <param name="right">The right value object.</param>
    /// <returns><c>true</c> if they are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);

    /// <summary>
    /// Returns the components used for equality comparison.
    /// Implementations should yield all fields that define the value.
    /// </summary>
    /// <returns>An enumerable of components used for equality.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();
}

