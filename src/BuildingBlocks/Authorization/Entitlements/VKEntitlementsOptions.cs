using VK.Blocks.Authorization.Entitlements.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Entitlements authorization feature.
/// </summary>
public sealed record VKEntitlementsOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAuthorizationBlock.BlockName}:{EntitlementsConstants.FeatureName}";

    /// <summary>
    /// Gets a value indicating whether the entitlements feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
