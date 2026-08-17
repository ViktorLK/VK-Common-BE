namespace VK.Blocks.Caching.ObjectCache.Internal;

/// <summary>
/// Represents an entry stored in the cache.
/// </summary>
internal sealed record CacheEntry<T>(
    T Value,
    DateTimeOffset? AbsoluteExpiration = null,
    TimeSpan? SlidingExpiration = null,
    int Version = 1)
{
    /// <summary>
    /// Gets a value indicating whether the entry is expired.
    /// </summary>
    public bool IsExpired(DateTimeOffset now) =>
        AbsoluteExpiration.HasValue && AbsoluteExpiration.Value <= now;
}
