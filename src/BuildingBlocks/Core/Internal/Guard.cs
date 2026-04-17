using System;
using System.Runtime.CompilerServices;

namespace VK.Blocks.Core.Internal;

/// <summary>
/// Provides static guard clause helpers to enforce preconditions
/// at the boundary of methods and constructors.
/// Throws descriptive exceptions immediately on violation.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Ensures a reference-type value is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if not null.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
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
    public static string NotNullOrWhiteSpace(string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value!;
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
            ? throw new ArgumentException($"Value cannot be the default value of {typeof(T).Name}.", paramName)
            : value;

    /// <summary>
    /// Ensures an integer is strictly positive (greater than zero).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if positive.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is zero or negative.</exception>
    public static int Positive(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null) =>
        value > 0 ? value : throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");

    /// <summary>
    /// Ensures an integer is non-negative (zero or greater).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <returns>The value if non-negative.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative.</exception>
    public static int NonNegative(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null) =>
        value >= 0 ? value : throw new ArgumentOutOfRangeException(paramName, value, "Value must be non-negative.");
}

