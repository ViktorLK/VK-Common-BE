namespace VK.Blocks.Core;

/// <summary>
/// Narrow, read-only projection contract for resolving the current tenant identifier from execution context.
/// Designed for components that only require <see cref="VKTenantId"/> coordinates without user awareness (e.g. EF Core filters, logging, diagnostics).
/// <para>
/// <b>Architecture SSoT Note:</b> This is an ISP projection of <see cref="IVKTenantCoordinate.TenantId"/>.
/// Implementations MUST project from <see cref="IVKAmbientContextAccessor"/> or active tenant resolution rather than maintaining a divergent resolution pipeline.
/// </para>
/// Follows AP.01, CS.01.
/// </summary>
public interface IVKTenantProvider
{
    /// <summary>
    /// Gets the current tenant identifier, or null if unassigned / single-tenant.
    /// </summary>
    VKTenantId? GetCurrentTenantId();

    /// <summary>
    /// Checks whether the current execution context is associated with an active tenant.
    /// </summary>
    bool HasTenantContext => GetCurrentTenantId() is not null;
}
