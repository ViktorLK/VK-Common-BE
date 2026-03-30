namespace VK.Blocks.Caching.Resilience.Penetration;

/// <summary>
/// Strategy for guarding against cache penetration by caching null values.
/// </summary>
public interface INullValueGuard
{
    /// <summary>
    /// Wraps a value for storage, potentially marking nulls if protection is enabled.
    /// </summary>
    object Wrap(object? value, bool enabled);

    /// <summary>
    /// Unwraps a stored value, restoring actual null if it was a null-marker.
    /// </summary>
    T? Unwrap<T>(object? value);
}
