namespace VK.Blocks.Caching.Core;

/// <summary>
/// Interface for standardized cache key building.
/// </summary>
public interface ICacheKeyBuilder
{
    /// <summary>
    /// Builds a full cache key using the configured prefix.
    /// </summary>
    /// <param name="key">The base key.</param>
    /// <returns>The prefixed cache key.</returns>
    string BuildKey(string key);
}
