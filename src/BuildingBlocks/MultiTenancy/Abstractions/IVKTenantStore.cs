using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Provides persistence-level access to tenant information.
/// Implementations may use a database, configuration file, or external service.
/// </summary>
public interface IVKTenantStore
{
    /// <summary>
    /// Retrieves tenant information by the specified tenant identifier.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing <see cref="IVKTenantInfo"/> if found; otherwise, a failure.</returns>
    Task<VKResult<IVKTenantInfo>> GetByIdAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves tenant information by the associated domain name.
    /// </summary>
    /// <param name="domain">The domain name associated with the tenant.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing <see cref="IVKTenantInfo"/> if found; otherwise, a failure.</returns>
    Task<VKResult<IVKTenantInfo>> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);
}
