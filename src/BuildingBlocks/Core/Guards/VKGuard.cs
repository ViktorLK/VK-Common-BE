using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace VK.Blocks.Core;

/// <summary>
/// Provides static guard clause helpers to enforce preconditions
/// at the boundary of methods and constructors.
/// Throws descriptive exceptions immediately on violation.
/// </summary>
public static class VKGuard
{
    /// <summary>
    /// Ensures a reference-type value is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if not null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotNull<T>(T? value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }

    /// <summary>
    /// Ensures a string is neither null nor whitespace.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if not null or whitespace.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or whitespace.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string NotNullOrWhiteSpace(string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value!;
    }

    /// <summary>
    /// Ensures a collection is not null and contains at least one element.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <param name="value">The collection to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The collection if not empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> contains no elements.</exception>
    public static T NotEmpty<T>(T? value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IEnumerable
    {
        ArgumentNullException.ThrowIfNull(value, paramName);

        if (!HasElements(value))
        {
            ThrowCollectionEmpty(paramName);
        }

        return value;
    }

    /// <summary>
    /// Ensures an integer is strictly positive (greater than zero).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if positive.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is zero or negative.</exception>
    public static int Positive(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null) =>
        value > 0 ? value : ThrowOutOfRange(paramName, value, "Value must be positive.");

    /// <summary>
    /// Ensures an integer is non-negative (zero or greater).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if non-negative.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative.</exception>
    public static int NonNegative(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null) =>
        value >= 0 ? value : ThrowOutOfRange(paramName, value, "Value must be non-negative.");

    /// <summary>
    /// Ensures a value is within the specified inclusive range.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The inclusive minimum value.</param>
    /// <param name="max">The inclusive maximum value.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if within range.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is outside the range.</exception>
    public static T InRange<T>(T value, T min, T max, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            ThrowOutOfRange(paramName, value, $"Value must be between {min} and {max}.");
        }
        return value;
    }

    /// <summary>
    /// Ensures a value-type value is not the default value for its type.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if not default.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> equals default.</exception>
    public static T NotDefault<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : struct =>
        value.Equals(default(T))
            ? ThrowDefaultValue<T>(paramName)
            : value;

    /// <summary>
    /// Ensures a Guid is not Guid.Empty.
    /// </summary>
    /// <param name="value">The Guid to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if not empty.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is Guid.Empty.</exception>
    public static Guid NotEmptyGuid(Guid value, [CallerArgumentExpression(nameof(value))] string? paramName = null) =>
        value == Guid.Empty
            ? ThrowEmptyGuid(paramName)
            : value;

    /// <summary>
    /// Ensures an enum value is defined within its type.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <param name="value">The enum value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if defined.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not defined in the enum type.</exception>
    public static TEnum EnumDefined<TEnum>(TEnum value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            ThrowEnumNotDefined(value, paramName);
        }
        return value;
    }

    /// <summary>
    /// Ensures a condition is not true.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="message">The exception message if condition is true.</param>
    /// <param name="paramName">The name of the parameter or expression being checked.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="condition"/> is true.</exception>
    public static void Against(bool condition, string message, [CallerArgumentExpression(nameof(condition))] string? paramName = null)
    {
        if (condition)
        {
            ThrowAgainst(message, paramName);
        }
    }

    private static bool HasElements(IEnumerable value) => value switch
    {
        ICollection collection => collection.Count > 0,
        _ => value.GetEnumerator().MoveNext()
    };

    [DoesNotReturn]
    private static void ThrowCollectionEmpty(string? paramName) =>
        throw new ArgumentException("Collection cannot be empty.", paramName);

    [DoesNotReturn]
    private static T ThrowOutOfRange<T>(string? paramName, T value, string message) =>
        throw new ArgumentOutOfRangeException(paramName, value, message);

    [DoesNotReturn]
    private static T ThrowDefaultValue<T>(string? paramName) =>
        throw new ArgumentException($"Value cannot be the default value of {typeof(T).Name}.", paramName);

    [DoesNotReturn]
    private static Guid ThrowEmptyGuid(string? paramName) =>
        throw new ArgumentException("Guid cannot be empty.", paramName);

    [DoesNotReturn]
    private static void ThrowEnumNotDefined<TEnum>(TEnum value, string? paramName) where TEnum : struct, Enum =>
        throw new ArgumentException($"Value '{value}' is not defined in enum '{typeof(TEnum).Name}'.", paramName);

    [DoesNotReturn]
    private static void ThrowAgainst(string message, string? paramName) =>
        throw new ArgumentException(message, paramName);
}
