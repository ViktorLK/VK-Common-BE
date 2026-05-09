using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Provides a mechanism to invalidate cached tenant information.
/// Used for live migration or dynamic metadata updates.
/// </summary>
public interface IVKTenantCacheInvalidator
{
    /// <summary>
    /// Invalidates the cache for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task InvalidateAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all cached tenant information.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
}
