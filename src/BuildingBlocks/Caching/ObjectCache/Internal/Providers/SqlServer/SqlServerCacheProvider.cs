using Microsoft.Extensions.Caching.Distributed;
using VK.Blocks.Core;

namespace VK.Blocks.Caching.ObjectCache.Internal.Providers.SqlServer;

/// <summary>
/// SQL Server cache provider based on IDistributedCache.
/// </summary>
internal sealed class SqlServerCacheProvider(IDistributedCache distributedCache) : ICacheProvider
{
    public string Name => "SqlServer";

    public async Task<VKResult<byte[]?>> GetAsync(string key, CancellationToken ct = default)
    {
        var value = await distributedCache.GetAsync(key, ct).ConfigureAwait(false);
        return VKResult.Success(value);
    }

    public async Task<VKResult> SetAsync(string key, byte[] value, VKCacheOptions options, CancellationToken ct = default)
    {
        var entryOptions = new DistributedCacheEntryOptions();

        if (options.Expiration.HasValue)
        {
            entryOptions.SetAbsoluteExpiration(options.Expiration.Value);
        }

        if (options.SlidingExpiration.HasValue)
        {
            entryOptions.SetSlidingExpiration(options.SlidingExpiration.Value);
        }

        await distributedCache.SetAsync(key, value, entryOptions, ct).ConfigureAwait(false);
        return VKResult.Success();
    }

    public async Task<VKResult> RemoveAsync(string key, CancellationToken ct = default)
    {
        await distributedCache.RemoveAsync(key, ct).ConfigureAwait(false);
        return VKResult.Success();
    }

    public Task<VKResult> RemoveByTagAsync(string tag, CancellationToken ct = default)
    {
        return Task.FromResult(VKResult.Success());
    }

    public async Task<VKResult> RefreshAsync(string key, CancellationToken ct = default)
    {
        await distributedCache.RefreshAsync(key, ct).ConfigureAwait(false);
        return VKResult.Success();
    }
}
