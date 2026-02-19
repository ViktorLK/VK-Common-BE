namespace VK.Blocks.Core.Primitives;

/// <summary>
/// Base class for value objects  Eimmutable, structurally-equal domain concepts
/// with no identity of their own (e.g., Money, Address, DateRange).
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    #region Methods

    /// <summary>
    /// Returns the components used for equality comparison.
    /// Implementations should yield all fields that define the value.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    #endregion

    #region Equality

    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject other)
            return false;
        if (GetType() != other.GetType())
            return false;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public bool Equals(ValueObject? other) =>
        other is not null && Equals((object)other);

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(0, (hash, component) => HashCode.Combine(hash, component));

    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);

    #endregion
}
