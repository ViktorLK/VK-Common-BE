namespace VK.Blocks.Authentication.Options;

/// <summary>
/// Root configuration options for the Authentication building block.
/// </summary>
public class VKAuthenticationOptions
{
    #region Fields

    /// <summary>
    /// The configuration section name for authentication options.
    /// </summary>
    public const string SectionName = "Authentication";

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether authentication is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the default authentication scheme.
    /// </summary>
    public string DefaultScheme { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the JWT validation options.
    /// </summary>
    public JwtValidationOptions Jwt { get; set; } = new();

    /// <summary>
    /// Gets or sets the OAuth options.
    /// </summary>
    public OAuthOptions OAuth { get; set; } = new();

    /// <summary>
    /// Gets or sets the API key scheme name.
    /// </summary>
    public string ApiKeySchemeName { get; set; } = "ApiKey";

    #endregion
}
