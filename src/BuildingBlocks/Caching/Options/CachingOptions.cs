namespace VK.Blocks.Caching.Options;

/// <summary>
/// Global caching configuration.
/// </summary>
public sealed class CachingOptions
{
    /// <summary>
    /// Default expiration time for cache entries.
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Prefix for all cache keys to avoid collisions.
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Selected cache provider type.
    /// </summary>
    public CacheProviderType Provider { get; set; } = CacheProviderType.Memory;
}
