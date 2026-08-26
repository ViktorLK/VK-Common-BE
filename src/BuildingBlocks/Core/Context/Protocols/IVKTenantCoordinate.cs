namespace VK.Blocks.Core;

/// <summary>
/// Minimal execution coordinate providing a strongly-typed tenant identifier.
/// Represents the fundamental spatial coordinate in the VK.Blocks identity stratification model.
/// Follows AP.01, AP.03.
/// </summary>
public interface IVKTenantCoordinate
{
    /// <summary>
    /// Gets the current strongly-typed TenantId coordinate.
    /// </summary>
    VKTenantId TenantId { get; }
}
