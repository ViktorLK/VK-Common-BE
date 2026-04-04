using VK.Blocks.Authentication.Features.ApiKeys;
using VK.Blocks.Authentication.Features.Jwt;
using VK.Blocks.Authentication.Features.OAuth;

namespace VK.Blocks.Authentication.DependencyInjection;

/// <summary>
/// Root configuration options for the Authentication building block.
/// </summary>
public sealed class VKAuthenticationOptions
{
    #region Fields

    /// <summary>
    /// The configuration section name for authentication options.
    /// </summary>
    public const string SectionName = "Authentication";

    /// <summary>
    /// The configuration section name for JWT options within the authentication block.
    /// </summary>
    public const string JwtSection = "Jwt";

    /// <summary>
    /// The configuration section name for API Key options within the authentication block.
    /// </summary>
    public const string ApiKeySection = "ApiKey";

    /// <summary>
    /// The configuration section name for OAuth options within the authentication block.
    /// </summary>
    public const string OAuthSection = "OAuth";

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
    /// Gets or sets the interval in minutes for periodic in-memory cache cleanup.
    /// Default is 10 minutes.
    /// </summary>
    public int InMemoryCleanupIntervalMinutes { get; set; } = 10;

    /// <summary>
    /// Gets or sets the configuration options for JWT tokens.
    /// </summary>
    public JwtOptions Jwt { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration options for API Keys.
    /// </summary>
    public ApiKeyOptions ApiKey { get; set; } = new();

    /// <summary>
    /// Gets or sets the configuration options for OAuth providers.
    /// </summary>
    public VKOAuthOptions OAuth { get; set; } = new();

    #endregion
}
