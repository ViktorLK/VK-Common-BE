namespace VK.Blocks.Core;

/// <summary>
/// Domain contract for entities and DTOs that are bound to a tenant security boundary.
/// Supports zero-code global multi-tenant isolation filters and EF Core interceptors.
/// </summary>
public interface IVKTenantScoped
{
    /// <summary>
    /// Gets the strongly-typed tenant identifier.
    /// </summary>
    VKTenantId TenantId { get; }
}
