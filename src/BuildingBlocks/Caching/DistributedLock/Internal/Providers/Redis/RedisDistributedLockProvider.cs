using StackExchange.Redis;
using VK.Blocks.Core;

namespace VK.Blocks.Caching.DistributedLock.Internal.Providers.Redis;

/// <summary>
/// Redis distributed lock provider.
/// </summary>
internal sealed class RedisDistributedLockProvider(
    IConnectionMultiplexer connectionMultiplexer,
    IVKGuidGenerator guidGenerator) : IVKDistributedLockProvider
{
    public IVKDistributedLock CreateLock(string resourceKey, TimeSpan? expiry = null)
    {
        return new RedisDistributedLock(
            connectionMultiplexer.GetDatabase(),
            resourceKey,
            guidGenerator.Create().ToString("N"),
            expiry ?? TimeSpan.FromSeconds(30));
    }
}
