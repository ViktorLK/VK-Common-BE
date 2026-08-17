using Microsoft.Extensions.Options;
using StackExchange.Redis;
using VK.Blocks.Core;

namespace VK.Blocks.Caching.ObjectCache.Internal.Providers.Hybrid;

/// <summary>
/// Hybrid cache provider that coordinates L1 (Memory) and L2 (Redis) providers.
/// </summary>
internal sealed class HybridCacheProvider : ICacheProvider, IDisposable
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

        if (redis is not null && _options.EnableL1)
        {
            _subscriber = redis.GetSubscriber();
            _subscriber.Subscribe(RedisChannel.Literal(_channelName), (channel, message) =>
            {
                var msg = message.ToString();
                if (msg.StartsWith("tag:", StringComparison.Ordinal))
                {
                    var tag = msg[4..];
                    _ = _l1Provider.RemoveByTagAsync(tag, CancellationToken.None);
                }
                else
                {
                    _ = _l1Provider.RemoveAsync(msg, CancellationToken.None);
                }
            });
        }
    }

    public string Name => "Hybrid";

    public async Task<VKResult<byte[]?>> GetAsync(string key, CancellationToken ct = default)
    {
        if (_options.EnableL1)
        {
            var l1Result = await _l1Provider.GetAsync(key, ct).ConfigureAwait(false);
            if (l1Result.IsSuccess && l1Result.Value is not null)
                return l1Result;
        }

        var l2Result = await _l2Provider.GetAsync(key, ct).ConfigureAwait(false);
        if (l2Result.IsFailure)
            return l2Result;

        if (l2Result.Value is not null && _options.EnableL1)
        {
            await _l1Provider.SetAsync(key, l2Result.Value, new VKCacheOptions { Expiration = _options.L1DefaultExpiration }, ct).ConfigureAwait(false);
        }

        return l2Result;
    }

    public async Task<VKResult> SetAsync(string key, byte[] value, VKCacheOptions options, CancellationToken ct = default)
    {
        if (_options.EnableL1)
        {
            var l1Options = options with { Expiration = options.Expiration ?? _options.L1DefaultExpiration };
            await _l1Provider.SetAsync(key, value, l1Options, ct).ConfigureAwait(false);
        }

        var l2Options = options with { Expiration = options.Expiration ?? _options.L2DefaultExpiration };
        var result = await _l2Provider.SetAsync(key, value, l2Options, ct).ConfigureAwait(false);

        if (result.IsSuccess && _subscriber is not null)
        {
            await _subscriber.PublishAsync(RedisChannel.Literal(_channelName), key).ConfigureAwait(false);
            foreach (var tag in options.Tags)
            {
                await _subscriber.PublishAsync(RedisChannel.Literal(_channelName), $"tag:{tag}").ConfigureAwait(false);
            }
        }

        return result;
    }

    public async Task<VKResult> RemoveAsync(string key, CancellationToken ct = default)
    {
        if (_options.EnableL1)
            await _l1Provider.RemoveAsync(key, ct).ConfigureAwait(false);
        var result = await _l2Provider.RemoveAsync(key, ct).ConfigureAwait(false);

        if (result.IsSuccess && _subscriber is not null)
        {
            await _subscriber.PublishAsync(RedisChannel.Literal(_channelName), key).ConfigureAwait(false);
        }

        return result;
    }

    public async Task<VKResult> RemoveByTagAsync(string tag, CancellationToken ct = default)
    {
        if (_options.EnableL1)
            await _l1Provider.RemoveByTagAsync(tag, ct).ConfigureAwait(false);
        var result = await _l2Provider.RemoveByTagAsync(tag, ct).ConfigureAwait(false);

        if (result.IsSuccess && _subscriber is not null)
        {
            await _subscriber.PublishAsync(RedisChannel.Literal(_channelName), $"tag:{tag}").ConfigureAwait(false);
        }

        return result;
    }

    public async Task<VKResult> RefreshAsync(string key, CancellationToken ct = default)
    {
        if (_options.EnableL1)
            await _l1Provider.RefreshAsync(key, ct).ConfigureAwait(false);
        return await _l2Provider.RefreshAsync(key, ct).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _subscriber?.UnsubscribeAll();
    }
}
