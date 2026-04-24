using System;
using System.Collections.Generic;

namespace VK.Blocks.Core;

/// <summary>
/// Base class for value objects 窶・immutable, structurally-equal domain concepts
/// with no identity of their own (e.g., Money, Address, DateRange).
/// </summary>
public abstract class VKValueObject : IEquatable<VKValueObject>
{
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not VKValueObject other)
        {
            return false;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        using IEnumerator<object?> left = GetEqualityComponents().GetEnumerator();
        using IEnumerator<object?> right = other.GetEqualityComponents().GetEnumerator();

        while (left.MoveNext())
        {
            if (!right.MoveNext() || !Equals(left.Current, right.Current))
            {
                return false;
            }
        }

        return !right.MoveNext();
    }

    /// <inheritdoc />
    public bool Equals(VKValueObject? other) =>
        other is not null && Equals((object)other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hashCode = default;

        foreach (object? component in GetEqualityComponents())
        {
            hashCode.Add(component);
        }

        return hashCode.ToHashCode();
    }

    /// <summary>
    /// Compares two <see cref="VKValueObject"/> instances for equality.
    /// </summary>
    /// <param name="left">The left value object.</param>
    /// <param name="right">The right value object.</param>
    /// <returns><c>true</c> if they are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(VKValueObject? left, VKValueObject? right) => Equals(left, right);
    public static bool operator !=(VKValueObject? left, VKValueObject? right) => !Equals(left, right);

    /// <summary>
    /// Returns the components used for equality comparison.
    /// Implementations should yield all fields that define the value.
    /// </summary>
    /// <returns>An enumerable of components used for equality.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();
}
