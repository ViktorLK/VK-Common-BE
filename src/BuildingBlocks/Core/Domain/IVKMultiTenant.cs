namespace VK.Blocks.Core;

/// <summary>
/// Defines an entity that belongs to a specific tenant in a multi-tenant system.
/// </summary>
public interface IVKMultiTenant
{
    /// <summary>
    /// Gets or sets the ID of the tenant that owns this entity.
    /// </summary>
    string? TenantId { get; }
}
