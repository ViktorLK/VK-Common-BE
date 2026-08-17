using VK.Blocks.Core;

namespace VK.Blocks.Caching.Resilience.Breakdown;

/// <summary>
/// Strategy contract for guarding against cache breakdown using distributed locks.
/// </summary>
internal interface ILockBasedGuard
{
    /// <summary>
    /// Executes the factory wrapped in a breakdown protection guard.
    /// </summary>
    Task<VKResult<VKCacheValue<T>>> ExecuteAsync<T>(
        IVKDistributedLockProvider lockProvider,
        string key,
        Func<CancellationToken, Task<VKResult<T>>> factory,
        Func<Task<VKResult<VKCacheValue<T>>>> cacheLookup,
        CancellationToken ct);
}
