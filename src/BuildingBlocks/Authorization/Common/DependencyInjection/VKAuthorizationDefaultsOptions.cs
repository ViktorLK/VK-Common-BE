using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Default configuration settings for the Authorization building block.
/// These values serve as fallbacks for all Authorization features.
/// Following BB.06: Modular Feature Pattern.
/// </summary>
[VKDefaults(typeof(VKAuthorizationBlock))]
public sealed partial record VKAuthorizationDefaultsOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the claim type used to extract roles (for SuperAdmin check).
    /// </summary>
    public string RoleClaimType { get; init; } = VKAuthorizationClaimTypes.Role;

    /// <summary>
    /// Gets the role name that can bypass all authorization checks (SuperAdmin).
    /// If null or empty, bypass is disabled.
    /// </summary>
    public string? SuperAdminRole { get; init; } = VKBlocksConstants.SuperAdminRole;

    /// <summary>
    /// Gets the claim type used to extract the tenant identifier.
    /// </summary>
    public string TenantClaimType { get; init; } = VKAuthorizationClaimTypes.TenantId;

    /// <summary>
    /// Gets the claim type used to extract the employee rank.
    /// </summary>
    public string RankClaimType { get; init; } = VKAuthorizationClaimTypes.Rank;

    /// <summary>
    /// Gets a value indicating whether tenant isolation is strictly enforced.
    /// If false, users with the SuperAdmin role can view all tenants.
    /// </summary>
    public bool StrictTenantIsolation { get; init; } = true;
}
