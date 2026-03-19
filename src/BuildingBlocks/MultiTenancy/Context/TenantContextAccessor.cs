using Microsoft.AspNetCore.Http;
using VK.Blocks.MultiTenancy.Abstractions.Contracts;

namespace VK.Blocks.MultiTenancy.Context;

/// <summary>
/// Provides access to the current <see cref="ITenantContext"/> from the
/// <see cref="IHttpContextAccessor"/>, following the same pattern as
/// <see cref="IHttpContextAccessor"/> itself.
/// </summary>
public sealed class TenantContextAccessor(ITenantContext tenantContext)
{
    #region Fields

    private readonly ITenantContext _tenantContext = tenantContext;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the current tenant information from the <see cref="ITenantContext"/>.
    /// </summary>
    public TenantInfo? CurrentTenant => _tenantContext.CurrentTenant;

    #endregion
}
