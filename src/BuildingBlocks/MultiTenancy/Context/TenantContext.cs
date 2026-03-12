using VK.Blocks.MultiTenancy.Abstractions.Contracts;

namespace VK.Blocks.MultiTenancy.Context;

/// <summary>
/// Scoped implementation of <see cref="ITenantContext"/> that stores
/// the resolved tenant information for the lifetime of a single request.
/// </summary>
public sealed class TenantContext : ITenantContext
{
    #region Properties

    /// <inheritdoc />
    public TenantInfo? CurrentTenant { get; private set; }

    /// <inheritdoc />
    public bool IsResolved => CurrentTenant is not null;

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the resolved tenant information for the current request.
    /// This method should only be called by the tenant resolution middleware.
    /// </summary>
    /// <param name="tenantInfo">The resolved tenant information.</param>
    public void SetTenant(TenantInfo tenantInfo)
    {
        CurrentTenant = tenantInfo ?? throw new ArgumentNullException(nameof(tenantInfo));
    }

    #endregion
}
