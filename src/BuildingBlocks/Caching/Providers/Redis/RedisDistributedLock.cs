using VK.Blocks.Caching.Abstractions;
using StackExchange.Redis;

namespace VK.Blocks.Caching.Providers.Redis;

/// <summary>
/// Distributed lock implementation using Redis SETNX.
/// </summary>
public sealed class RedisDistributedLock(
    IDatabase database,
    string resourceKey,
    string lockValue,
    TimeSpan expiry) : IDistributedLock
{
    public bool IsAcquired { get; private set; }

    public async Task<bool> AcquireAsync(CancellationToken cancellationToken = default)
    {
        IsAcquired = await database.StringSetAsync(
            resourceKey,
            lockValue,
            expiry,
            When.NotExists);

        return IsAcquired;
    }

    public async Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAcquired) return;

        // Use Lua script to ensure we only release our own lock
        string script = "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";
        await database.ScriptEvaluateAsync(script, [new RedisKey(resourceKey)], [new RedisValue(lockValue)]);

        IsAcquired = false;
    }

    public async ValueTask DisposeAsync()
    {
        await ReleaseAsync();
    }
}
