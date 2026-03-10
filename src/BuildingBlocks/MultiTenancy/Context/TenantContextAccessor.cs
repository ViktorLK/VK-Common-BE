using Microsoft.AspNetCore.Http;
using VK.Blocks.MultiTenancy.Abstractions.Contracts;

namespace VK.Blocks.MultiTenancy.Context;

/// <summary>
/// Provides access to the current <see cref="ITenantContext"/> from the
/// <see cref="IHttpContextAccessor"/>, following the same pattern as
/// <see cref="IHttpContextAccessor"/> itself.
/// </summary>
public sealed class TenantContextAccessor(IHttpContextAccessor httpContextAccessor)
{
    #region Fields

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the current tenant information from the <see cref="ITenantContext"/>
    /// stored in the request's service scope, or <c>null</c> if unavailable.
    /// </summary>
    public TenantInfo? CurrentTenant
    {
        get
        {
            var tenantContext = _httpContextAccessor.HttpContext?
                .RequestServices.GetService(typeof(ITenantContext)) as ITenantContext;

            return tenantContext?.CurrentTenant;
        }
    }

    #endregion
}
