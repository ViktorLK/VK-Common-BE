using VK.Blocks.MultiTenancy.Abstractions.Contracts;

namespace VK.Blocks.MultiTenancy.Abstractions;

/// <summary>
/// Provides persistence-level access to tenant information.
/// Implementations may use a database, configuration file, or external service.
/// </summary>
public interface ITenantStore
{
    /// <summary>
    /// Retrieves tenant information by the specified tenant identifier.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The <see cref="TenantInfo"/> if found; otherwise, <c>null</c>.</returns>
    Task<TenantInfo?> GetByIdAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves tenant information by the associated domain name.
    /// </summary>
    /// <param name="domain">The domain name associated with the tenant.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The <see cref="TenantInfo"/> if found; otherwise, <c>null</c>.</returns>
    Task<TenantInfo?> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);
}
