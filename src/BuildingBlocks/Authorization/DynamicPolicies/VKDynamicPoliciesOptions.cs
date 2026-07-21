using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DynamicPolicies;

/// <summary>
/// Configuration options for the Dynamic Policies authorization feature.
/// </summary>

public sealed partial record VKDynamicPoliciesOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the dynamic policies feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Request-specific override for Requirement.</summary>
    [VKRequestOverride]
    public Microsoft.AspNetCore.Authorization.IAuthorizationRequirement? Requirement { get; init; }

    /// <summary>Request-specific override for the resource context to evaluate attributes against.</summary>
    [VKRequestOverride]
    public object? Resource { get; init; }
}
