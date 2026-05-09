namespace VK.Blocks.MultiTenancy;

/// <summary>
/// Defines a factory for creating <see cref="IVKTenantInfo"/> instances during resolution.
/// Allows customization of the concrete tenant type used in the context.
/// </summary>
public interface IVKTenantInfoFactory
{
    /// <summary>
    /// Creates a tenant information object for the specified identifier.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="name">The display name for the tenant (often the same as ID during resolution).</param>
    /// <returns>An instance of <see cref="IVKTenantInfo"/>.</returns>
    IVKTenantInfo Create(string tenantId, string? name = null);
}
