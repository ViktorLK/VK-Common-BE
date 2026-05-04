using VK.Blocks.Authorization.TenantIsolation.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Tenant Isolation authorization feature.
/// </summary>
public sealed record VKTenantIsolationOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAuthorizationBlock.BlockName}:{TenantIsolationConstants.FeatureName}";

    /// <summary>
    /// Gets a value indicating whether the tenant isolation feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the claim type used to extract the tenant identifier.
    /// </summary>
    public string TenantClaimType { get; init; } = VKAuthorizationClaimTypes.TenantId;

    /// <summary>
    /// Gets a value indicating whether tenant isolation is strictly enforced.
    /// If false, users with the SuperAdmin role can view all tenants.
    /// </summary>
    public bool StrictTenantIsolation { get; init; } = true;
}
