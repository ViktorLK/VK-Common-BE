namespace VK.Blocks.Core;

/// <summary>
/// Minimal execution context providing a non-null strongly-typed TenantId coordinate.
/// Level 1 in the VK.Blocks 3-tier Identity Stratification model.
/// Defaults to <see cref="VKTenantId.Default"/> when in single-tenant or unassigned context.
/// Follows AP.01, CS.01, CS.06.
/// </summary>
public interface IVKTenantContext
{
    /// <summary>
    /// Gets the current strongly-typed TenantId. Never null; defaults to <see cref="VKTenantId.Default"/>.
    /// </summary>
    VKTenantId TenantId { get; }
}
