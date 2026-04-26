using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Configuration options for OAuth providers.
/// </summary>
public sealed record VKOAuthOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the configuration section name for OAuth options.
    /// </summary>
    public static string SectionName => $"{VKAuthenticationOptions.SectionName}:{VKAuthenticationOptions.OAuthSection}";

    /// <summary>
    /// Gets or sets a value indicating whether OAuth provider registration is enabled.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the dictionary of OAuth provider settings, keyed by provider name (e.g. "GitHub").
    /// </summary>
    public Dictionary<string, VKOAuthProviderOptions> Providers { get; init; } = [];
}
