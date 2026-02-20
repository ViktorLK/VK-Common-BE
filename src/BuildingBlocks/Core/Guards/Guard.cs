namespace VK.Blocks.Core.Guards;

/// <summary>
/// Provides static guard clause helpers to enforce preconditions
/// at the boundary of methods and constructors.
/// Throws descriptive exceptions immediately on violation.
/// </summary>
public static class Guard
{
    #region Methods

    /// <summary>Ensures a reference-type value is not null.</summary>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static T NotNull<T>(T? value, string paramName) where T : class =>
        value ?? throw new ArgumentNullException(paramName);

    /// <summary>Ensures a string is neither null nor whitespace.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or whitespace.</exception>
    public static string NotNullOrWhiteSpace(string? value, string paramName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be null or whitespace.", paramName)
            : value;

    /// <summary>Ensures a value-type value is not the default value for its type.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> equals default.</exception>
    public static T NotDefault<T>(T value, string paramName) where T : struct =>
        value.Equals(default(T))
            ? throw new ArgumentException($"Value cannot be the default value of {typeof(T).Name}.", paramName)
            : value;

    /// <summary>Ensures an integer is strictly positive (greater than zero).</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is zero or negative.</exception>
    public static int Positive(int value, string paramName) =>
        value > 0 ? value : throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");

    /// <summary>Ensures an integer is non-negative (zero or greater).</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is negative.</exception>
    public static int NonNegative(int value, string paramName) =>
        value >= 0 ? value : throw new ArgumentOutOfRangeException(paramName, value, "Value must be non-negative.");

    #endregion
}
