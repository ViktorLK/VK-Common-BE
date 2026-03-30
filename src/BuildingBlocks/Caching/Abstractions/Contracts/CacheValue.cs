namespace VK.Blocks.Caching.Abstractions.Contracts;

/// <summary>
/// A wrapper for cached values that allows distinguishing between a miss and a hit-with-null.
/// Enforces non-nullability for the Result pattern while allowing the underlying value to be null.
/// </summary>
/// <typeparam name="T">The type of the cached value.</typeparam>
public sealed record CacheValue<T>(T? Value, bool IsHit)
{
    private static readonly CacheValue<T> _miss = new(default, false);

    /// <summary> Gets a value indicating a cache miss. </summary>
    public static CacheValue<T> NoValue => _miss;

    /// <summary> Creates a cache hit. </summary>
    public static CacheValue<T> ValueOf(T? value) => new(value, true);
    
    /// <summary> Implicit conversion to the underlying value. </summary>
    public static implicit operator T?(CacheValue<T> cacheValue) => cacheValue.Value;

    /// <summary> Returns true if this represents a cache hit. </summary>
    public bool HasValue => IsHit;
}
