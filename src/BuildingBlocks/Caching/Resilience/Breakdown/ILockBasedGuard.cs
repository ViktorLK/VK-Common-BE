using VK.Blocks.Caching.Abstractions;
using VK.Blocks.Caching.Abstractions.Contracts;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Caching.Resilience.Breakdown;

/// <summary>
/// Strategy for guarding against cache breakdown using distributed locks.
/// </summary>
public interface ILockBasedGuard
{
    /// <summary>
    /// Executes a factory method with distributed lock protection.
    /// </summary>
    Task<Result<CacheValue<T>>> ExecuteAsync<T>(
        IDistributedLockProvider lockProvider,
        string key,
        Func<CancellationToken, Task<Result<T>>> factory,
        Func<Task<Result<CacheValue<T>>>> cacheLookup,
        CancellationToken ct);
}
