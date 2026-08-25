namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Default implementation of <see cref="IVKTenantProvider"/> that projects directly from the unified <see cref="AsyncLocalExecutionContext"/>.
/// Follows AP.01, AP.03.
/// </summary>
internal sealed class DefaultIdentityTenantProvider : IVKTenantProvider
{
    /// <inheritdoc />
    public VKTenantId? GetCurrentTenantId()
    {
        return AsyncLocalExecutionContext.Current.TenantId;
    }
}
