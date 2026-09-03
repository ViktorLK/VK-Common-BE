using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for caching resilience patterns including Stale-While-Revalidate (SWR) and Cache Fallback.
/// Follows [AP.01], [CS.01], [CS.03].
/// </summary>
public interface IVKCacheResilienceProvider
{
    /// <summary>
    /// Executes a read operation using Stale-While-Revalidate (SWR): returns cached value even if stale while triggering background refresh.
    /// </summary>
    Task<VKResult<T>> ExecuteWithStaleWhileRevalidateAsync<T>(
        string cacheKey,
        Func<CancellationToken, Task<VKResult<T>>> fetchSource,
        TimeSpan freshDuration,
        TimeSpan staleDuration,
        CancellationToken cancellationToken = default);
}
