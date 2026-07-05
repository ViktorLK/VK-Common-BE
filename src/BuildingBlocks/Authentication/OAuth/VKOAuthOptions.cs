using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Configuration options for OAuth providers.
/// </summary>
[VKFeature(typeof(VKAuthenticationBlock))]
public sealed partial record VKOAuthOptions : IVKToggleableBlockOptions
{

    /// <summary>
    /// Gets or sets a value indicating whether OAuth provider registration is enabled.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the dictionary of OAuth provider settings, keyed by provider name (e.g. "GitHub").
    /// </summary>
    public Dictionary<string, VKOAuthProviderOptions> Providers { get; init; } = [];
}
