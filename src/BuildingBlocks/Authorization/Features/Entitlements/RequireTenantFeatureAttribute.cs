using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization.Features.Entitlements;

/// <summary>
/// Specifies that the class or method that this attribute is applied to requires a specific tenant feature.
/// </summary>
/// <param name="featureName">The name of the required tenant feature.</param>
public sealed class RequireTenantFeatureAttribute(string featureName) : AuthorizeAttribute
{
    /// <summary>
    /// Gets the name of the required tenant feature.
    /// </summary>
    public string FeatureName { get; } = featureName;

    /// <summary>
    /// Gets or sets the policy name that corresponds to this requirement.
    /// Internal use only.
    /// </summary>
    public new string Policy
    {
        get => base.Policy ?? $"TenantFeature_{FeatureName}";
        set => base.Policy = value;
    }
}
