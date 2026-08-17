using VK.Blocks.Core;

namespace VK.Blocks.Caching.Resilience.Breakdown;

/// <summary>
/// Strategy for guarding against cache breakdown using distributed locks.
/// Ensures only one concurrent request executes the factory while others wait.
/// </summary>
internal sealed class LockBasedGuard : ILockBasedGuard
{
    /// <inheritdoc />
    public async Task<VKResult<VKCacheValue<T>>> ExecuteAsync<T>(
        IVKDistributedLockProvider lockProvider,
        string key,
        Func<CancellationToken, Task<VKResult<T>>> factory,
        Func<Task<VKResult<VKCacheValue<T>>>> cacheLookup,
        CancellationToken ct)
    {
        // Double check cache before locking
        var cachedResult = await cacheLookup().ConfigureAwait(false);
        if (cachedResult.IsSuccess && cachedResult.Value.HasValue)
            return cachedResult;

        await using var @lock = lockProvider.CreateLock($"lock:{key}");
        if (await @lock.AcquireAsync(ct).ConfigureAwait(false))
        {
            try
            {
                // Double check cache after acquiring lock
                cachedResult = await cacheLookup().ConfigureAwait(false);
                if (cachedResult.IsSuccess && cachedResult.Value!.HasValue)
                    return cachedResult;

                var res = await factory(ct).ConfigureAwait(false);
                return res.IsSuccess ? VKResult.Success(VKCacheValue<T>.ValueOf(res.Value)) : VKResult.Failure<VKCacheValue<T>>(res.Errors);
            }
            finally
            {
                await @lock.ReleaseAsync(ct).ConfigureAwait(false);
            }
        }

        // Final check after lock acquisition failure
        cachedResult = await cacheLookup().ConfigureAwait(false);
        if (cachedResult.IsSuccess && cachedResult.Value.HasValue)
            return cachedResult;

        return VKResult.Failure<VKCacheValue<T>>(VKCachingErrors.LockAcquisitionFailed);
    }
}
