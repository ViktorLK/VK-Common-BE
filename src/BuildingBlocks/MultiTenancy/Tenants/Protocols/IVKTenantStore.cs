using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Provides persistence-level access to retrieve <see cref="VKTenantInfo"/> descriptors for runtime resolution.
/// Follows AP.01, CS.01, CS.03.
/// </summary>
public interface IVKTenantStore
{
    /// <summary>
    /// Retrieves a tenant descriptor by the specified tenant identifier.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing <see cref="VKTenantInfo"/> if found; otherwise, a failure.</returns>
    Task<VKResult<VKTenantInfo>> GetByIdAsync(VKTenantId tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a tenant descriptor by the associated domain name.
    /// </summary>
    /// <param name="domain">The domain name associated with the tenant.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing <see cref="VKTenantInfo"/> if found; otherwise, a failure.</returns>
    Task<VKResult<VKTenantInfo>> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active tenant descriptors in the system.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing a list of <see cref="VKTenantInfo"/>.</returns>
    Task<VKResult<IReadOnlyList<VKTenantInfo>>> GetActiveTenantsAsync(CancellationToken cancellationToken = default);
}
