using VK.Blocks.Core;

namespace VK.Blocks.Caching;

/// <summary>
/// Unified entry point for all caching operations.
/// </summary>
public interface IVKCacheBlock
{
    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    Task<VKResult<VKCacheValue<T>>> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    Task<VKResult> SetAsync<T>(string key, T value, VKCacheOptions? options = null, CancellationToken ct = default);

    /// <summary>
    /// Gets a value from the cache, or fetches it from the source if not found.
    /// </summary>
    Task<VKResult<T>> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        VKCacheOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    Task<VKResult> RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Removes all cache entries associated with the specified tag.
    /// </summary>
    Task<VKResult> RemoveByTagAsync(string tag, CancellationToken ct = default);

    /// <summary>
    /// Creates a distributed lock for the specified resource.
    /// </summary>
    Task<VKResult<IVKDistributedLock>> AcquireLockAsync(string resourceKey, VKLockOptions? options = null, CancellationToken ct = default);
}
