using StackExchange.Redis;

namespace VK.Blocks.Caching.DistributedLock.Internal.Providers.Redis;

/// <summary>
/// Distributed lock implementation using Redis SETNX.
/// </summary>
internal sealed class RedisDistributedLock(
    IDatabase database,
    string resourceKey,
    string lockValue,
    TimeSpan expiry) : IVKDistributedLock
{
    public bool IsAcquired { get; private set; }

    public string ResourceKey => resourceKey;

    public async Task<bool> AcquireAsync(CancellationToken ct = default)
    {
        IsAcquired = await database.StringSetAsync(
            resourceKey,
            lockValue,
            expiry,
            When.NotExists).WaitAsync(ct).ConfigureAwait(false);

        return IsAcquired;
    }

    public async Task ReleaseAsync(CancellationToken ct = default)
    {
        if (!IsAcquired)
            return;

        // Use Lua script to ensure we only release our own lock
        string script = "if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end";
        await database.ScriptEvaluateAsync(script, [new RedisKey(resourceKey)], [new RedisValue(lockValue)]).WaitAsync(ct).ConfigureAwait(false);

        IsAcquired = false;
    }

    public async ValueTask DisposeAsync()
    {
        await ReleaseAsync().ConfigureAwait(false);
    }
}
