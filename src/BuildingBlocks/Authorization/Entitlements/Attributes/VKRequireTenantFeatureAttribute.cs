using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization;

/// <summary>
/// Specifies that the class or method that this attribute is applied to requires a specific tenant feature.
/// </summary>
/// <param name="featureName">The name of the required tenant feature.</param>
public sealed class VKRequireTenantFeatureAttribute(string featureName) : AuthorizeAttribute, IAuthorizationRequirementData
{
    /// <summary>
    /// Gets the name of the required tenant feature.
    /// </summary>
    public string FeatureName { get; } = featureName;

    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new VKTenantFeatureRequirement(FeatureName);
    }
}
