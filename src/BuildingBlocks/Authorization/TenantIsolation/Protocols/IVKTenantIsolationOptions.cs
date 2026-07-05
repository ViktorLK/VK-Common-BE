using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines global static options for Tenant Isolation.
/// </summary>
public interface IVKTenantIsolationOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the claim type used to identify the user's tenant ID.
    /// </summary>
    string? TenantClaimType { get; init; }

    /// <summary>
    /// Gets a value indicating whether strict tenant isolation is enforced.
    /// </summary>
    bool? StrictTenantIsolation { get; init; }
}
