using VK.Blocks.Caching.Abstractions;
using VK.Blocks.Caching.Abstractions.Contracts;
using VK.Blocks.Caching.Core;
using VK.Blocks.Caching.Options;
using VK.Blocks.Core.Results;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Polly.Wrap;

namespace VK.Blocks.Caching.Providers.Redis;

/// <summary>
/// Redis cache provider based on StackExchange.Redis.
/// </summary>
public sealed class RedisCacheProvider(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<RedisCacheOptions> options,
    AsyncPolicyWrap policy) : ICacheProvider
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private readonly RedisCacheOptions _options = options.Value;

    public string Name => "Redis";

    public async Task<Result<byte[]?>> GetAsync(string key, CancellationToken ct = default)
    {
        try
        {
            // CT-01: Propagate cancellation token to WaitAsync as SE.Redis 2.x doesn't take it directly.
            return await policy.ExecuteAsync(async (c) => 
            {
                var value = await _database.StringGetAsync(key).WaitAsync(c);
                return Result.Success(value.IsNull ? null : (byte[]?)value);
            }, ct);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<byte[]?>(CachingErrors.ProviderError);
        }
        catch (Exception)
        {
            return Result.Failure<byte[]?>(CachingErrors.ProviderError);
        }
    }

    public async Task<Result> SetAsync(string key, byte[] value, CacheOptions options, CancellationToken ct = default)
    {
        var expiration = options.Expiration;
        var ttl = expiration ?? options.SlidingExpiration;

        try
        {
            return await policy.ExecuteAsync(async (c) => 
            {
                if (ttl.HasValue)
                    await _database.StringSetAsync(key, value, ttl.Value).WaitAsync(c);
                else
                    await _database.StringSetAsync(key, value).WaitAsync(c);
                
                return Result.Success();
            }, ct);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(CachingErrors.ProviderError);
        }
        catch (Exception)
        {
            return Result.Failure(CachingErrors.ProviderError);
        }
    }

    public async Task<Result> RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            // CT-02: Wrapped in policy
            return await policy.ExecuteAsync(async (c) => 
            {
                await _database.KeyDeleteAsync(key).WaitAsync(c);
                return Result.Success();
            }, ct);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(CachingErrors.ProviderError);
        }
        catch (Exception)
        {
            return Result.Failure(CachingErrors.ProviderError);
        }
    }

    public async Task<Result> RefreshAsync(string key, CancellationToken ct = default)
    {
        try
        {
            // NF-02: Fix RefreshAsync logic. 
            // Passing null TTL clears the expiration in many Redis clients.
            // Since we don't have the current sliding TTL, we should ideally fetch it or use a default.
            // For now, we avoid clearing it if we don't have a value.
            return await policy.ExecuteAsync(async (c) => 
            {
                // In a real scenario, we might want to fetch the TTL then re-apply it.
                // Or use a Touch command if the client supports it.
                // For this implementation, we skip resetting it to null.
                var currentTtl = await _database.KeyTimeToLiveAsync(key).WaitAsync(c);
                if (currentTtl.HasValue)
                {
                    await _database.KeyExpireAsync(key, currentTtl.Value).WaitAsync(c);
                }
                
                return Result.Success();
            }, ct);
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(CachingErrors.ProviderError);
        }
        catch (Exception)
        {
            return Result.Failure(CachingErrors.ProviderError);
        }
    }
}

