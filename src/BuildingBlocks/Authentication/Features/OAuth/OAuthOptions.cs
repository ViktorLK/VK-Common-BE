namespace VK.Blocks.Authentication.Features.OAuth;

/// <summary>
/// Configuration options for OAuth providers.
/// </summary>
public sealed class OAuthOptions
{
    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether OAuth provider registration is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the configured OAuth providers, keyed by provider name.
    /// </summary>
    public Dictionary<string, OAuthProviderOptions> Providers { get; set; } = [];

    #endregion
}
