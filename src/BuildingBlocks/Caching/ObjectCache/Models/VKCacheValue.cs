using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.Caching;

/// <summary>
/// Represents a value retrieved from the cache, distinguishing between a cached null, a cache miss, and a valid cached value.
/// </summary>
public sealed record VKCacheValue<T>
{
    private readonly T? _value;

    private VKCacheValue(bool hasValue, T? value)
    {
        HasValue = hasValue;
        _value = value;
    }

    /// <summary>
    /// Gets a value indicating whether a value is present in the cache.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; }

    /// <summary>
    /// Gets the cached value if present.
    /// </summary>
    public T? Value => HasValue ? _value : default;

    /// <summary>
    /// Represents a cache miss.
    /// </summary>
    public static readonly VKCacheValue<T> NoValue = new(false, default);

    /// <summary>
    /// Creates a cache value representing a cache hit.
    /// </summary>
    public static VKCacheValue<T> ValueOf(T? value) => new(true, value);
}
