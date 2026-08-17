namespace VK.Blocks.Caching.Resilience.Penetration;

/// <summary>
/// Strategy contract for guarding against cache penetration.
/// </summary>
internal interface INullValueGuard
{
    /// <summary>
    /// Wraps a value to prevent cache penetration.
    /// </summary>
    object Wrap(object? value, bool enabled);

    /// <summary>
    /// Unwraps a cached value.
    /// </summary>
    T? Unwrap<T>(object? value);
}
