using VK.Blocks.Authorization.DynamicPolicies.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DynamicPolicies;

/// <summary>
/// Configuration options for the Dynamic Policies authorization feature.
/// </summary>
public sealed record VKDynamicPoliciesOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAuthorizationBlock.BlockName}:{DynamicPoliciesConstants.FeatureName}";

    /// <summary>
    /// Gets a value indicating whether the dynamic policies feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
