namespace VK.Blocks.Caching.Options;

/// <summary>
/// Supported cache provider types.
/// </summary>
public enum CacheProviderType
{
    /// <summary>
    /// In-memory cache (L1).
    /// </summary>
    Memory,

    /// <summary>
    /// Redis distributed cache (L2).
    /// </summary>
    Redis,

    /// <summary>
    /// Hybrid cache (L1 Memory + L2 Redis).
    /// </summary>
    Hybrid,

    /// <summary>
    /// SQL Server distributed cache.
    /// </summary>
    SqlServer
}
