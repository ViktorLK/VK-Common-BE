namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Minimal pure tenant-only execution context (Level 1).
/// Follows AP.01, AP.03.
/// </summary>
internal sealed class DefaultTenantContext(VKTenantId tenantId) : IVKTenantContext
{
    public static DefaultTenantContext DefaultInstance { get; } = new(VKTenantId.Default);

    public VKTenantId TenantId { get; init; } = tenantId;
}
