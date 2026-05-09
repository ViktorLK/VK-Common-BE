using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Context.Internal;

/// <summary>
/// Implementation of <see cref="IVKTenantProvider"/> that delegates to <see cref="IVKTenantContext"/>.
/// </summary>
internal sealed class TenantContextTenantProvider(IVKTenantContext tenantContext) : IVKTenantProvider
{
    private readonly IVKTenantContext _tenantContext = VKGuard.NotNull(tenantContext);

    /// <inheritdoc />
    public string? GetCurrentTenantId()
    {
        return _tenantContext.CurrentTenant?.Id;
    }
}
