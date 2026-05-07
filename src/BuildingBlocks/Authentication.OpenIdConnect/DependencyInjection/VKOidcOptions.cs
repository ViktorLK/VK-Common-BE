using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect;

/// <summary>
/// Configuration options for the OpenIdConnect building block.
/// Complies with BB.05.
/// </summary>
public sealed record VKOidcOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name.
    /// Complies with AP.04.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:OpenIdConnect";

    /// <summary>
    /// Gets or sets a value indicating whether the OIDC block is enabled.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to save tokens to the authentication session.
    /// </summary>
    public bool SaveTokens { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether metadata must be fetched via HTTPS.
    /// </summary>
    public bool RequireHttpsMetadata { get; init; } = true;

    /// <summary>
    /// Gets or sets the timeout in seconds for backchannel HTTP calls.
    /// </summary>
    public int BackchannelTimeoutSeconds { get; init; } = 60;

    /// <summary>
    /// Gets or sets the dictionary of OIDC provider settings, keyed by provider name.
    /// </summary>
    public Dictionary<string, VKOidcProviderOptions> Providers { get; init; } = [];
}

