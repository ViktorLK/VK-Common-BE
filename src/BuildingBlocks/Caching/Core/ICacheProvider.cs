using VK.Blocks.Caching.Abstractions.Contracts;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Caching.Core;

/// <summary>
/// Internal interface for cache provider implementations (Memory, Redis, etc.)
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    Task<Result<byte[]?>> GetAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    Task<Result> SetAsync(string key, byte[] value, CacheOptions options, CancellationToken ct = default);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    Task<Result> RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Refreshes the expiration of a key.
    /// </summary>
    Task<Result> RefreshAsync(string key, CancellationToken ct = default);
}

