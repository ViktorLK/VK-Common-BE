using VK.Blocks.Core;

namespace VK.Blocks.Caching.ObjectCache.Internal;

/// <summary>
/// Internal interface for cache provider implementations.
/// </summary>
internal interface ICacheProvider
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    Task<VKResult<byte[]?>> GetAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    Task<VKResult> SetAsync(string key, byte[] value, VKCacheOptions options, CancellationToken ct = default);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    Task<VKResult> RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Removes all cache entries associated with the specified tag.
    /// </summary>
    Task<VKResult> RemoveByTagAsync(string tag, CancellationToken ct = default);

    /// <summary>
    /// Refreshes the expiration of a key.
    /// </summary>
    Task<VKResult> RefreshAsync(string key, CancellationToken ct = default);
}
