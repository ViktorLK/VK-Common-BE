using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Represents a requirement that a specific tenant feature must be enabled.
/// </summary>
/// <param name="featureName">The name of the required feature.</param>
public sealed class VKTenantFeatureRequirement(string featureName) : IVKAuthorizationRequirement
{
    /// <summary>
    /// Gets the name of the feature required by this instance.
    /// </summary>
    public string FeatureName { get; } = featureName;

    /// <inheritdoc />
    public VKError DefaultError => VKAuthorizationErrors.FeatureDenied;
}
