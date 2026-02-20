namespace VK.Blocks.MultiTenancy.Abstractions;

/// <summary>
/// Provides access to the current tenant's identifier.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the current tenant identifier.
    /// </summary>
    /// <returns>The tenant identifier, or null if no tenant context is available.</returns>
    string? GetCurrentTenantId();

    /// <summary>
    /// Checks whether the request is executed within a multi-tenant context.
    /// </summary>
    bool HasTenantContext => !string.IsNullOrWhiteSpace(GetCurrentTenantId());
}
