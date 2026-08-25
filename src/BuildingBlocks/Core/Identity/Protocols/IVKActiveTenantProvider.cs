using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Provides access to active tenant list for background workers and system jobs without depending on MultiTenancy block.
/// (AP.01) (CS.01)
/// </summary>
public interface IVKActiveTenantProvider
{
    /// <summary>
    /// Gets all currently active tenant identifiers in the system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing a list of active <see cref="VKTenantId"/> instances.</returns>
    Task<VKResult<IReadOnlyList<VKTenantId>>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);
}
