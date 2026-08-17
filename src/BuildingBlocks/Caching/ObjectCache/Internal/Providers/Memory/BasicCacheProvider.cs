using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using VK.Blocks.Core;

namespace VK.Blocks.Caching.ObjectCache.Internal.Providers.Memory;

/// <summary>
/// Basic in-memory cache provider based on IMemoryCache.
/// </summary>
internal sealed class BasicCacheProvider(IMemoryCache memoryCache) : ICacheProvider
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _tagKeys = new();

    public string Name => "Memory";

    public Task<VKResult<byte[]?>> GetAsync(string key, CancellationToken ct = default)
    {
        return Task.FromResult(VKResult.Success<byte[]?>(memoryCache.Get<byte[]>(key)));
    }

    public Task<VKResult> SetAsync(string key, byte[] value, VKCacheOptions options, CancellationToken ct = default)
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

        // Manage tags
        foreach (var tag in options.Tags)
        {
            var keys = _tagKeys.GetOrAdd(tag, _ => new ConcurrentDictionary<string, byte>());
            keys.TryAdd(key, 0);
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> RemoveAsync(string key, CancellationToken ct = default)
    {
        memoryCache.Remove(key);

        // Remove key from all tags
        foreach (var keys in _tagKeys.Values)
        {
            keys.TryRemove(key, out _);
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> RemoveByTagAsync(string tag, CancellationToken ct = default)
    {
        if (_tagKeys.TryRemove(tag, out var keys))
        {
            foreach (var key in keys.Keys)
            {
                memoryCache.Remove(key);
            }
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> RefreshAsync(string key, CancellationToken ct = default)
    {
        _ = memoryCache.Get(key);
        return Task.FromResult(VKResult.Success());
    }
}
