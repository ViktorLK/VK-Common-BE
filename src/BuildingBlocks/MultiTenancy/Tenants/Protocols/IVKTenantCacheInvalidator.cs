using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Provides a mechanism to invalidate cached tenant profiles when tenant data changes.
/// Follows AP.01, CS.01.
/// </summary>
public interface IVKTenantCacheInvalidator
{
    /// <summary>
    /// Evicts the cached entry for the specified tenant identifier.
    /// </summary>
    /// <param name="tenantId">The identifier of the tenant to evict.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="VKResult"/> indicating success or failure.</returns>
    Task<VKResult> InvalidateAsync(VKTenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evicts all cached tenant entries.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="VKResult"/> indicating success or failure.</returns>
    Task<VKResult> InvalidateAllAsync(CancellationToken cancellationToken = default);
}
