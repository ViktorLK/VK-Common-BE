using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.MultiTenancy.Context;

namespace VK.Blocks.MultiTenancy.DependencyInjection;

/// <summary>
/// <see cref="ITenantProvider"/> implementation that delegates to the
/// resolved <see cref="ITenantContext"/> instead of reading headers directly.
/// This bridges the legacy <see cref="ITenantProvider"/> contract with the new
/// resolution pipeline architecture.
/// </summary>
internal sealed class TenantContextTenantProvider(ITenantContext tenantContext) : ITenantProvider
{
    #region Fields

    private readonly ITenantContext _tenantContext = tenantContext;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public string? GetCurrentTenantId()
    {
        return _tenantContext.CurrentTenant?.Id;
    }

    #endregion
}
