using VK.Blocks.Caching.Abstractions;
using StackExchange.Redis;

namespace VK.Blocks.Caching.Providers.Redis;

/// <summary>
/// Redis distributed lock provider.
/// </summary>
public sealed class RedisDistributedLockProvider(IConnectionMultiplexer connectionMultiplexer) : IDistributedLockProvider
{
    public IDistributedLock CreateLock(string resourceKey, TimeSpan? expiry = null)
    {
        return new RedisDistributedLock(
            connectionMultiplexer.GetDatabase(),
            resourceKey,
            Guid.NewGuid().ToString("N"),
            expiry ?? TimeSpan.FromSeconds(30));
    }
}
