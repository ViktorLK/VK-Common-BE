using VK.Blocks.Core;

namespace VK.Blocks.MultiTenancy.Context.Internal;

/// <summary>
/// Scoped implementation of <see cref="IVKTenantContext"/> that stores
/// the resolved tenant information for the lifetime of a single request.
/// </summary>
internal sealed class TenantContext : IVKTenantContext, IVKTenantContextSetter
{
    /// <inheritdoc />
    public IVKTenantInfo? CurrentTenant { get; private set; }

    /// <inheritdoc />
    public bool IsResolved => CurrentTenant is not null;

    /// <summary>
    /// Sets the resolved tenant information for the current request.
    /// This method should only be called by the tenant resolution middleware.
    /// </summary>
    /// <param name="tenantInfo">The resolved tenant information.</param>
    public void SetTenant(IVKTenantInfo tenantInfo)
    {
        CurrentTenant = VKGuard.NotNull(tenantInfo);
    }
}
