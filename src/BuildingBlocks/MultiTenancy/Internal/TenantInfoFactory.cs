using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Internal;

/// <summary>
/// Default implementation of <see cref="IVKTenantInfoFactory"/> that creates <see cref="VKTenantInfo"/>.
/// </summary>
internal sealed class TenantInfoFactory : IVKTenantInfoFactory
{
    /// <inheritdoc />
    public IVKTenantInfo Create(VKTenantId tenantId, string? name = null)
    {
        return new VKTenantInfo(tenantId, name ?? tenantId.ToString());
    }
}
