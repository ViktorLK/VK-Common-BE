using VK.Blocks.Caching.Abstractions;
using VK.Blocks.Caching.Abstractions.Contracts;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Caching.Resilience.Breakdown;

/// <summary>
/// Strategy for guarding against cache breakdown using distributed locks.
/// Ensures only one concurrent request executes the factory while others wait.
/// </summary>
public sealed class LockBasedGuard : ILockBasedGuard
{
    /// <inheritdoc />
    public async Task<Result<CacheValue<T>>> ExecuteAsync<T>(
        IDistributedLockProvider lockProvider,
        string key,
        Func<CancellationToken, Task<Result<T>>> factory,
        Func<Task<Result<CacheValue<T>>>> cacheLookup,
        CancellationToken ct)
    {
        // Double check cache before locking
        var cachedResult = await cacheLookup();
        if (cachedResult.IsSuccess && cachedResult.Value.HasValue) return cachedResult;

        await using var @lock = lockProvider.CreateLock($"lock:{key}");
        if (await @lock.AcquireAsync(ct))
        {
            try
            {
                // Double check cache after acquiring lock
                cachedResult = await cacheLookup();
                if (cachedResult.IsSuccess && cachedResult.Value!.HasValue) return cachedResult;

                var res = await factory(ct);
                return res.IsSuccess ? Result.Success(CacheValue<T>.ValueOf(res.Value)) : Result.Failure<CacheValue<T>>(res.Errors);
            }
            finally
            {
                await @lock.ReleaseAsync(ct);
            }
        }

        // Final check after lock acquisition failure
        cachedResult = await cacheLookup();
        if (cachedResult.IsSuccess && cachedResult.Value.HasValue) return cachedResult;

        return Result.Failure<CacheValue<T>>(CachingErrors.LockAcquisitionFailed);
    }
}
