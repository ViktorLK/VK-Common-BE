using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Configuration options for the authentication building block.
/// </summary>
public sealed record VKAuthenticationOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for authentication options.
    /// </summary>
    public static string SectionName => VKBlocksConstants.VKBlocksConfigPrefix + "Authentication";

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

    /// <summary>
    /// Gets or sets a value indicating whether authentication is enabled.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the default authentication scheme.
    /// </summary>
    public string DefaultScheme { get; init; } = "Bearer";

    /// <summary>
    /// Gets or sets the interval in minutes for periodic in-memory cache cleanup.
    /// Default is 10 minutes.
    /// </summary>
    public int InMemoryCleanupIntervalMinutes { get; init; } = 10;

    /// <summary>
    /// Gets or sets the configuration options for JWT tokens.
    /// </summary>
    public VKJwtOptions Jwt { get; init; } = new();

    /// <summary>
    /// Gets or sets the configuration options for API Keys.
    /// </summary>
    public VKApiKeyOptions ApiKey { get; init; } = new();

    /// <summary>
    /// Gets or sets the configuration options for OAuth providers.
    /// </summary>
    public VKOAuthOptions OAuth { get; init; } = new();
}
