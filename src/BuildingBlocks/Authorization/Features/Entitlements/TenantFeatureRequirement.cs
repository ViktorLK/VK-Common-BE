using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization.Features.Entitlements;

/// <summary>
/// Represents a requirement that a specific tenant feature must be enabled.
/// </summary>
/// <param name="featureName">The name of the required feature.</param>
public sealed class TenantFeatureRequirement(string featureName) : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the name of the feature required by this instance.
    /// </summary>
    public string FeatureName { get; } = featureName;
}
