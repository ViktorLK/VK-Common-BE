using VK.Blocks.Caching.Abstractions.Contracts;
using VK.Blocks.Caching.Core;
using VK.Blocks.Core.Results;
using Microsoft.Extensions.Caching.Memory;

namespace VK.Blocks.Caching.Providers.Memory;

/// <summary>
/// Memory cache provider based on IMemoryCache.
/// </summary>
public sealed class MemoryCacheProvider(IMemoryCache memoryCache) : ICacheProvider
{
    public string Name => "Memory";

    public Task<Result<byte[]?>> GetAsync(string key, CancellationToken ct = default)
    {
        return Task.FromResult(Result.Success<byte[]?>(memoryCache.Get<byte[]>(key)));
    }

    public Task<Result> SetAsync(string key, byte[] value, CacheOptions options, CancellationToken ct = default)
    {
        var entryOptions = new MemoryCacheEntryOptions();

        if (options.Expiration.HasValue)
        {
            entryOptions.SetAbsoluteExpiration(options.Expiration.Value);
        }

        if (options.SlidingExpiration.HasValue)
        {
            entryOptions.SetSlidingExpiration(options.SlidingExpiration.Value);
        }

        memoryCache.Set(key, value, entryOptions);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> RemoveAsync(string key, CancellationToken ct = default)
    {
        memoryCache.Remove(key);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> RefreshAsync(string key, CancellationToken ct = default)
    {
        // IMemoryCache naturally handles sliding expiration if set during SetAsync.
        // Explicit Get triggers refresh for sliding.
        _ = memoryCache.Get(key);
        return Task.FromResult(Result.Success());
    }
}

