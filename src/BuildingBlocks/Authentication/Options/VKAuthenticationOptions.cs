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
    /// Gets or sets the lifetime of a refresh token in days. Defaults to 30.
    /// </summary>
    public int RefreshTokenLifetimeDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the API key scheme name.
    /// </summary>
    public string ApiKeySchemeName { get; set; } = "ApiKey";

    /// <summary>
    /// Gets or sets the maximum number of API key validations allowed per minute per key.
    /// Defaults to 60. Set to 0 to disable the key entirely.
    /// </summary>
    public int ApiKeyRateLimitPerMinute { get; set; } = 60;

    #endregion
}
