using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Context.Internal;

/// <summary>
/// Provides access to the current <see cref="IVKTenantContext"/> from the
/// <see cref="Microsoft.AspNetCore.Http.IHttpContextAccessor"/> pattern.
/// </summary>
internal sealed class TenantContextAccessor(IVKTenantContext tenantContext)
{
    private readonly IVKTenantContext _tenantContext = VKGuard.NotNull(tenantContext);

    /// <summary>
    /// Gets the current tenant information from the <see cref="IVKTenantContext"/>.
    /// </summary>
    public IVKTenantInfo? CurrentTenant => _tenantContext.CurrentTenant;
}
