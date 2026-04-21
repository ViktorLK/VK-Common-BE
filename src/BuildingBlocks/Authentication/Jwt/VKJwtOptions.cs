using Microsoft.AspNetCore.Authentication.JwtBearer;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Configuration options for JWT tokens.
/// </summary>
public sealed record VKJwtOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for Jwt options.
    /// </summary>
    public static string SectionName => VKAuthenticationOptions.SectionName + ":" + VKAuthenticationOptions.JwtSection;

    /// <summary>
    /// Gets or sets a value indicating whether JWT validation is enabled.
    /// </summary>
    public bool Enabled { get; init; } = false;

    /// <summary>
    /// Gets or sets the authentication scheme name for JWT.
    /// Default is "Bearer".
    /// </summary>
    public string SchemeName { get; init; } = JwtBearerDefaults.AuthenticationScheme;

    /// <summary>
    /// Gets or sets the authentication mode for token validation.
    /// Default is Symmetric (self-issued).
    /// </summary>
    public VKJwtAuthMode AuthMode { get; init; } = VKJwtAuthMode.Symmetric;

    /// <summary>
    /// Gets or sets the issuer of the token.
    /// Initializing as string.Empty ensures a strong validation contract by preventing null reference exceptions
    /// and enforcing that these required configuration values must be explicitly provided or validated.
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the intended audience of the token.
    /// Initializing as string.Empty ensures a strong validation contract by preventing null reference exceptions
    /// and enforcing that these required configuration values must be explicitly provided or validated.
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the secret key used to sign the token.
    /// Initializing as string.Empty ensures a strong validation contract by preventing null reference exceptions
    /// and enforcing that these required configuration values must be explicitly provided or validated.
    /// Required only when AuthMode is Symmetric.
    /// </summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the authority URL (OIDC Discovery Base URL).
    /// Required when AuthMode is OidcDiscovery.
    /// </summary>
    public string Authority { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the custom metadata address for OIDC discovery, if it cannot be inferred from Authority.
    /// </summary>
    public string? MetadataAddress { get; init; }

    /// <summary>
    /// Gets or sets the expiration time of the access token in minutes.
    /// </summary>
    public int ExpiryMinutes { get; init; } = 60;

    /// <summary>
    /// Gets or sets the clock skew in seconds allowed when validating a token.
    /// </summary>
    public double ClockSkewSeconds { get; init; }

    /// <summary>
    /// Gets or sets the lifetime of a refresh token in days. Defaults to 30.
    /// </summary>
    public int RefreshTokenLifetimeDays { get; init; } = 30;

    /// <summary>
    /// Determines whether the JWT feature should be activated based on configuration.
    /// </summary>
    internal bool IsFeatureActivated()
    {
        return Enabled && AuthMode switch
        {
            VKJwtAuthMode.Symmetric => !string.IsNullOrEmpty(SecretKey),
            VKJwtAuthMode.OidcDiscovery => !string.IsNullOrEmpty(Authority),
            _ => false
        };
    }
}





