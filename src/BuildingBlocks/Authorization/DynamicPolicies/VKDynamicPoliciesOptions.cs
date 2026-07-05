using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DynamicPolicies;

/// <summary>
/// Configuration options for the Dynamic Policies authorization feature.
/// </summary>
[VKFeature(typeof(VKAuthorizationBlock), GenerateArgs = true)]
public sealed partial record VKDynamicPoliciesOptions : IVKDynamicPoliciesOptions
{
    /// <summary>
    /// Gets a value indicating whether the dynamic policies feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
