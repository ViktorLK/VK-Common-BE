using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Entitlements authorization feature.
/// </summary>
[VKFeature(typeof(VKAuthorizationBlock))]
public sealed partial record VKEntitlementsOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the entitlements feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
