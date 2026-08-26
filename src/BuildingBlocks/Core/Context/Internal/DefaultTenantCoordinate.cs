namespace VK.Blocks.Core.Context.Internal;

/// <summary>
/// Minimal pure tenant-only execution coordinate (Level 1).
/// Follows AP.01, AP.03.
/// </summary>
internal sealed record DefaultTenantCoordinate(VKTenantId TenantId) : IVKTenantCoordinate;
