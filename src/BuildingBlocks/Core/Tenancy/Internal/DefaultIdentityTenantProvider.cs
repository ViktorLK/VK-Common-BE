namespace VK.Blocks.Core.Tenancy.Internal;

/// <summary>
/// Default implementation of <see cref="IVKTenantProvider"/> that projects directly from the unified <see cref="VKAmbientExecutionContext"/>.
/// Follows AP.01, AP.03.
/// </summary>
internal sealed class DefaultIdentityTenantProvider : IVKTenantProvider
{
    /// <inheritdoc />
    public VKTenantId? GetCurrentTenantId()
    {
        return VKAmbientExecutionContext.Current?.Tenant?.TenantId;
    }
}
