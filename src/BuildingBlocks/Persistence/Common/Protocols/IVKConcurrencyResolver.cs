using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

/// <summary>
/// Defines a contract to resolve optimistic concurrency conflicts and apply retry strategies.
/// </summary>
public interface IVKConcurrencyResolver
{
    /// <summary>
    /// Resolves a concurrency conflict by retrying the save changes action.
    /// </summary>
    /// <param name="saveAction">The delegate representing SaveChangesAsync.</param>
    /// <param name="retryCount">The maximum number of retries allowed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the number of updated state entries.</returns>
    // [CS.03]
    Task<VKResult<int>> ResolveAndRetryAsync(
        Func<CancellationToken, Task<int>> saveAction,
        int retryCount = 3,
        CancellationToken cancellationToken = default);
}
