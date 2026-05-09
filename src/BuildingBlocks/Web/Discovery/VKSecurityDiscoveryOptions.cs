using VK.Blocks.Core;

namespace VK.Blocks.Web;

/// <summary>
/// Options for configuring the security discovery diagnostic features.
/// </summary>
public sealed record VKSecurityDiscoveryOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:Web:Discovery";

    /// <summary>
    /// Gets or sets a value indicating whether the security topology endpoint is enabled.
    /// Default is false for security reasons.
    /// </summary>
    public bool Enabled { get; init; } = false;
}
