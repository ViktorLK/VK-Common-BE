using VK.Blocks.Caching.Abstractions.Contracts;
using VK.Blocks.Caching.Core;
using VK.Blocks.Core.Results;
using Microsoft.Extensions.Caching.Distributed;

namespace VK.Blocks.Caching.Providers.SqlServer;

/// <summary>
/// SQL Server cache provider based on IDistributedCache.
/// </summary>
public sealed class SqlServerCacheProvider(IDistributedCache distributedCache) : ICacheProvider
{
    public string Name => "SqlServer";

    public async Task<Result<byte[]?>> GetAsync(string key, CancellationToken ct = default)
    {
        var value = await distributedCache.GetAsync(key, ct);
        return Result.Success(value);
    }

    public async Task<Result> SetAsync(string key, byte[] value, CacheOptions options, CancellationToken ct = default)
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

        await distributedCache.SetAsync(key, value, entryOptions, ct);
        return Result.Success();
    }

    public async Task<Result> RemoveAsync(string key, CancellationToken ct = default)
    {
        await distributedCache.RemoveAsync(key, ct);
        return Result.Success();
    }

    public async Task<Result> RefreshAsync(string key, CancellationToken ct = default)
    {
        await distributedCache.RefreshAsync(key, ct);
        return Result.Success();
    }
}

