namespace VK.Blocks.Core.Domain;

/// <summary>
/// Defines an entity that belongs to a specific tenant in a multi-tenant system with a setter.
/// </summary>
public interface IMultiTenantEntity : IMultiTenant
{
    /// <summary>
    /// Gets or sets the ID of the tenant that owns this entity.
    /// </summary>
    new string? TenantId { get; set; }
}

