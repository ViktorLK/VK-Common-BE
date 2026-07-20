using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Entitlements authorization feature.
/// </summary>

public sealed partial record VKEntitlementsOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the entitlements feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
