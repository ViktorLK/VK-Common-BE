namespace VK.Blocks.Authentication.Features.OAuth;

/// <summary>
/// Represents the configuration options for a single OAuth provider.
/// </summary>
public sealed class OAuthProviderOptions
{
    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether this provider is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the custom authentication scheme name.
    /// If not provided, the configuration key (e.g. "GitHub") will be used.
    /// </summary>
    public string? SchemeName { get; set; }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authority URL.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback path.
    /// </summary>
    public string CallbackPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the requested scopes.
    /// </summary>
    public List<string> Scopes { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether to retrieve additional claims from the user info endpoint.
    /// Necessary for providers that don't include all profile data in the ID Token (e.g. GitHub).
    /// </summary>
    public bool GetClaimsFromUserInfoEndpoint { get; set; }

    #endregion
}
