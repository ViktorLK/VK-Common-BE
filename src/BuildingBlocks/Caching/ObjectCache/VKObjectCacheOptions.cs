using VK.Blocks.Core;

namespace VK.Blocks.Caching;

/// <summary>
/// Options for the ObjectCache feature slice.
/// </summary>

public sealed partial record VKObjectCacheOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the cache provider type.
    /// </summary>
    public CacheProviderType Provider { get; init; } = CacheProviderType.Memory;

    /// <summary>
    /// Gets the default expiration time for cache entries.
    /// </summary>
    public TimeSpan DefaultExpiration { get; init; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets the prefix for all cache keys.
    /// </summary>
    public string KeyPrefix { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Redis specific configuration.
    /// </summary>
    public RedisCacheOptions Redis { get; init; } = new();

    /// <summary>
    /// Gets the Hybrid specific configuration.
    /// </summary>
    public HybridCacheOptions Hybrid { get; init; } = new();
}
