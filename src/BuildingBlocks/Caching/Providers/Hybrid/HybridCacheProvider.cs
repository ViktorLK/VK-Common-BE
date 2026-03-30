using VK.Blocks.Caching.Abstractions.Contracts;
using VK.Blocks.Caching.Core;
using VK.Blocks.Caching.Options;
using VK.Blocks.Core.Results;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace VK.Blocks.Caching.Providers.Hybrid;

/// <summary>
/// Hybrid cache provider that coordinates L1 (Memory) and L2 (Redis) providers.
/// </summary>
public sealed class HybridCacheProvider : ICacheProvider, IDisposable
{
    private readonly ICacheProvider _l1Provider;
    private readonly ICacheProvider _l2Provider;
    private readonly HybridCacheOptions _options;
    private readonly ISubscriber? _subscriber;
    private readonly string _channelName = "cache:invalidation";

    public HybridCacheProvider(
        ICacheProvider l1Provider,
        ICacheProvider l2Provider,
        IOptions<HybridCacheOptions> hybridOptions,
        IConnectionMultiplexer? redis = null)
    {
        _l1Provider = l1Provider;
        _l2Provider = l2Provider;
        _options = hybridOptions.Value;
        
        if (redis != null && _options.EnableL1)
        {
            _subscriber = redis.GetSubscriber();
            _subscriber.Subscribe(RedisChannel.Literal(_channelName), (channel, message) => 
            {
                var key = message.ToString();
                // Avoid async void where possible, but here we just trigger a background clear
                _ = _l1Provider.RemoveAsync(key, CancellationToken.None);
            });
        }
    }

    public string Name => "Hybrid";

    public async Task<Result<byte[]?>> GetAsync(string key, CancellationToken ct = default)
    {
        // Try L1
        if (_options.EnableL1)
        {
            var l1Result = await _l1Provider.GetAsync(key, ct);
            if (l1Result.IsSuccess && l1Result.Value != null) return l1Result;
        }

        // Try L2
        var l2Result = await _l2Provider.GetAsync(key, ct);
        if (l2Result.IsFailure) return l2Result;

        if (l2Result.Value != null && _options.EnableL1)
        {
            // Backfill L1
            await _l1Provider.SetAsync(key, l2Result.Value, new CacheOptions { Expiration = _options.L1DefaultExpiration }, ct);
        }

        return l2Result;
    }

    public async Task<Result> SetAsync(string key, byte[] value, CacheOptions options, CancellationToken ct = default)
    {
        // Set L1
        if (_options.EnableL1)
        {
            var l1Options = options with { Expiration = options.Expiration ?? _options.L1DefaultExpiration };
            await _l1Provider.SetAsync(key, value, l1Options, ct);
        }

        // Set L2
        var l2Options = options with { Expiration = options.Expiration ?? _options.L2DefaultExpiration };
        var result = await _l2Provider.SetAsync(key, value, l2Options, ct);

        if (result.IsSuccess && _subscriber != null)
        {
            await _subscriber.PublishAsync(RedisChannel.Literal(_channelName), key);
        }

        return result;
    }

    public async Task<Result> RemoveAsync(string key, CancellationToken ct = default)
    {
        if (_options.EnableL1) await _l1Provider.RemoveAsync(key, ct);
        var result = await _l2Provider.RemoveAsync(key, ct);

        if (result.IsSuccess && _subscriber != null)
        {
            await _subscriber.PublishAsync(RedisChannel.Literal(_channelName), key);
        }

        return result;
    }

    public async Task<Result> RefreshAsync(string key, CancellationToken ct = default)
    {
        if (_options.EnableL1) await _l1Provider.RefreshAsync(key, ct);
        return await _l2Provider.RefreshAsync(key, ct);
    }

    public void Dispose()
    {
        _subscriber?.UnsubscribeAll();
    }
}

