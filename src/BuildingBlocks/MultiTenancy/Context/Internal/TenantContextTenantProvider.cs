using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Context.Internal;

/// <summary>
/// Implementation of <see cref="IVKTenantProvider"/> that delegates to <see cref="IVKTenantContext"/>.
/// </summary>
internal sealed class TenantContextTenantProvider(IVKTenantContext tenantContext) : IVKTenantProvider
{
    private readonly IVKTenantContext _tenantContext = VKGuard.NotNull(tenantContext);

    /// <inheritdoc />
    public VKTenantId? GetCurrentTenantId()
    {
        return _tenantContext.IsResolved ? _tenantContext.TenantId : null;
    }
}
