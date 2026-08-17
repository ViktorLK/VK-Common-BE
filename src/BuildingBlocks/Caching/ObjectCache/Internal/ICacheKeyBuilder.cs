namespace VK.Blocks.Caching.ObjectCache.Internal;

/// <summary>
/// Internal interface for building standardized cache keys.
/// </summary>
internal interface ICacheKeyBuilder
{
    /// <summary>
    /// Builds a full cache key.
    /// </summary>
    string BuildKey(string key);
}
