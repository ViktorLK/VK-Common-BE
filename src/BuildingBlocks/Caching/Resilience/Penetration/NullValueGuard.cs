namespace VK.Blocks.Caching.Resilience.Penetration;

/// <summary>
/// Strategy for guarding against cache penetration.
/// </summary>
public sealed class NullValueGuard : INullValueGuard
{
    private readonly NullValueMarker _marker = new();

    /// <inheritdoc />
    public object Wrap(object? value, bool enabled)
    {
        if (!enabled) return value!;
        return value ?? _marker;
    }

    /// <inheritdoc />
    public T? Unwrap<T>(object? value)
    {
        if (value is NullValueMarker) return default;
        return (T?)value;
    }
}
