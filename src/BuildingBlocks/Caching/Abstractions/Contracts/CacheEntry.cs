namespace VK.Blocks.Caching.Abstractions.Contracts;

/// <summary>
/// Represents an entry stored in the cache.
/// </summary>
/// <typeparam name="T">The type of the cached value.</typeparam>
public sealed record CacheEntry<T>(
    T Value,
    DateTimeOffset? AbsoluteExpiration = null,
    TimeSpan? SlidingExpiration = null)
{
    /// <summary>
    /// Gets a value indicating whether the entry is expired.
    /// </summary>
    public bool IsExpired(DateTimeOffset now) =>
        AbsoluteExpiration.HasValue && AbsoluteExpiration.Value <= now;
}
