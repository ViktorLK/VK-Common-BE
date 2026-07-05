namespace VK.Blocks.Authorization;

/// <summary>
/// Defines request-level overrides and target parameters for Tenant Isolation.
/// </summary>
public interface IVKTenantIsolationOverrides
{
    /// <summary>
    /// Gets the claim type used to identify the user's tenant ID, overriding the default setting.
    /// </summary>
    string? TenantClaimType { get; init; }

    /// <summary>
    /// Gets a value indicating whether strict tenant isolation is enforced, overriding the default setting.
    /// </summary>
    bool? StrictTenantIsolation { get; init; }

    /// <summary>
    /// Gets the target tenant ID to check against.
    /// </summary>
    string? TargetTenantId { get; init; }
}
