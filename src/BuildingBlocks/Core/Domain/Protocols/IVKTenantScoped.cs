namespace VK.Blocks.Core;

/// <summary>
/// Domain entity marker protocol identifying entities that belong to a specific tenant boundary.
/// Enables automated multi-tenant global filtering and data partition enforcement.
/// Follows AP.01, AP.03, CS.08.
/// </summary>
public interface IVKTenantScoped
{
    /// <summary>
    /// Gets the unique identifier of the tenant owning this entity.
    /// </summary>
    VKTenantId TenantId { get; }
}
