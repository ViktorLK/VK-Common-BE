using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Tenant Isolation authorization feature.
/// </summary>

public sealed partial record VKTenantIsolationOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the tenant isolation feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the claim type used to extract the tenant identifier.
    /// If null, the global default TenantClaimType is used.
    /// </summary>
    [VKRequestOverride]
    public string? TenantClaimType { get; init; }

    /// <summary>
    /// Gets a value indicating whether tenant isolation is strictly enforced.
    /// If false, users with the SuperAdmin role can view all tenants.
    /// If null, the global default StrictTenantIsolation is used.
    /// </summary>
    [VKRequestOverride]
    public bool? StrictTenantIsolation { get; init; }

    /// <summary>Request-specific override for TargetTenantId.</summary>
    [VKRequestOverride]
    public string? TargetTenantId { get; init; }
}
