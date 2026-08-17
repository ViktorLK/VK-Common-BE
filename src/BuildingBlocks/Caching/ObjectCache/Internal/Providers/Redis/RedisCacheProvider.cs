using Microsoft.Extensions.Options;
using Polly.Wrap;
using StackExchange.Redis;
using VK.Blocks.Core;

namespace VK.Blocks.Caching.ObjectCache.Internal.Providers.Redis;

/// <summary>
/// Redis cache provider based on StackExchange.Redis.
/// </summary>
internal sealed class RedisCacheProvider(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<RedisCacheOptions> options,
    AsyncPolicyWrap policy) : ICacheProvider
{
    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();
    private readonly RedisCacheOptions _options = options.Value;

    public string Name => "Redis";

    public async Task<VKResult<byte[]?>> GetAsync(string key, CancellationToken ct = default)
    {
        try
        {
            return await policy.ExecuteAsync(async (c) =>
            {
                var value = await _database.StringGetAsync(key).WaitAsync(c).ConfigureAwait(false);
                return VKResult.Success(value.IsNull ? null : (byte[]?)value);
            }, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return VKResult.Failure<byte[]?>(VKCachingErrors.ProviderError);
        }
        catch (Exception)
        {
            return VKResult.Failure<byte[]?>(VKCachingErrors.ProviderError);
        }
    }

    public async Task<VKResult> SetAsync(string key, byte[] value, VKCacheOptions options, CancellationToken ct = default)
    {
        var expiration = options.Expiration;
        var ttl = expiration ?? options.SlidingExpiration;

        try
        {
            return await policy.ExecuteAsync(async (c) =>
            {
                if (ttl.HasValue)
                    await _database.StringSetAsync(key, value, ttl.Value).WaitAsync(c).ConfigureAwait(false);
                else
                    await _database.StringSetAsync(key, value).WaitAsync(c).ConfigureAwait(false);

                // Add to tags sets
                foreach (var tag in options.Tags)
                {
                    var tagKey = $"tag:{tag}";
                    await _database.SetAddAsync(tagKey, key).WaitAsync(c).ConfigureAwait(false);
                    if (ttl.HasValue)
                    {
                        await _database.KeyExpireAsync(tagKey, ttl.Value).WaitAsync(c).ConfigureAwait(false);
                    }
                }

                return VKResult.Success();
            }, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return VKResult.Failure(VKCachingErrors.ProviderError);
        }
        catch (Exception)
        {
            return VKResult.Failure(VKCachingErrors.ProviderError);
        }
    }

    public async Task<VKResult> RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            return await policy.ExecuteAsync(async (c) =>
            {
                await _database.KeyDeleteAsync(key).WaitAsync(c).ConfigureAwait(false);
                return VKResult.Success();
            }, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return VKResult.Failure(VKCachingErrors.ProviderError);
        }
        catch (Exception)
        {
            return VKResult.Failure(VKCachingErrors.ProviderError);
        }
    }

    public async Task<VKResult> RemoveByTagAsync(string tag, CancellationToken ct = default)
    {
        try
        {
            return await policy.ExecuteAsync(async (c) =>
            {
                var tagKey = $"tag:{tag}";
                var members = await _database.SetMembersAsync(tagKey).WaitAsync(c).ConfigureAwait(false);
                if (members.Length > 0)
                {
                    var keys = members.Select(m => (RedisKey)m.ToString()).ToArray();
                    await _database.KeyDeleteAsync(keys).WaitAsync(c).ConfigureAwait(false);
                }
                await _database.KeyDeleteAsync(tagKey).WaitAsync(c).ConfigureAwait(false);
                return VKResult.Success();
            }, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return VKResult.Failure(VKCachingErrors.ProviderError);
        }
        catch (Exception)
        {
            return VKResult.Failure(VKCachingErrors.ProviderError);
        }
    }

    public async Task<VKResult> RefreshAsync(string key, CancellationToken ct = default)
    {
        try
        {
            return await policy.ExecuteAsync(async (c) =>
            {
                var currentTtl = await _database.KeyTimeToLiveAsync(key).WaitAsync(c).ConfigureAwait(false);
                if (currentTtl.HasValue)
                {
                    await _database.KeyExpireAsync(key, currentTtl.Value).WaitAsync(c).ConfigureAwait(false);
                }

                return VKResult.Success();
            }, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return VKResult.Failure(VKCachingErrors.ProviderError);
        }
        catch (Exception)
        {
            return VKResult.Failure(VKCachingErrors.ProviderError);
        }
    }
}
