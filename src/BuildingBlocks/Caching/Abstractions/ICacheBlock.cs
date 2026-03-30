using VK.Blocks.Caching.Abstractions.Contracts;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Caching.Abstractions;

/// <summary>
/// Unified entry point for all caching operations.
/// </summary>
public interface ICacheBlock
{
    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    Task<Result<CacheValue<T>>> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    Task<Result> SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken ct = default);

    /// <summary>
    /// Gets a value from the cache, or fetches it from the source if not found.
    /// </summary>
    Task<Result<T>> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        CacheOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    Task<Result> RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Creates a distributed lock for the specified resource.
    /// </summary>
    Task<Result<IDistributedLock>> AcquireLockAsync(string resourceKey, LockOptions? options = null, CancellationToken ct = default);
}
