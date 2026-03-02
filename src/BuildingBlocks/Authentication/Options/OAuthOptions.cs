using System.Collections.Generic;

namespace VK.Blocks.Authentication.Options;

/// <summary>
/// Configuration options for OAuth providers.
/// </summary>
public class OAuthOptions
{
    #region Properties

    /// <summary>
    /// Gets or sets the Azure B2C configuration.
    /// </summary>
    public ProviderOptions? AzureB2C { get; set; }

    /// <summary>
    /// Gets or sets the configuration for Google OAuth.
    /// </summary>
    public ProviderOptions? Google { get; set; }

    /// <summary>
    /// Gets or sets the configuration for GitHub OAuth.
    /// </summary>
    public ProviderOptions? GitHub { get; set; }

    #endregion
}

/// <summary>
/// Represents the configuration options for a single OAuth provider.
/// </summary>
public class ProviderOptions
{
    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether this provider is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the client identifier.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the authority URL.
    /// </summary>
    public string? Authority { get; set; }

    /// <summary>
    /// Gets or sets the callback path.
    /// </summary>
    public string? CallbackPath { get; set; }

    /// <summary>
    /// Gets or sets the requested scopes.
    /// </summary>
    public List<string> Scopes { get; set; } = new();

    #endregion
}
