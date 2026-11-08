namespace VK.Blocks.Caching;

/// <summary>
/// Protocol for building standardized cache keys.
/// Follows AP.03.
/// </summary>
public interface IVKCacheKeyBuilder
{
    /// <summary>
    /// Builds a full cache key enforcing tenant isolation.
    /// </summary>
    string BuildKey(string key);
}

