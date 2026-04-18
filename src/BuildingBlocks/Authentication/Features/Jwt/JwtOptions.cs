using Microsoft.AspNetCore.Authentication.JwtBearer;
using VK.Blocks.Authentication.Features.Jwt.Metadata;

namespace VK.Blocks.Authentication.Features.Jwt;

/// <summary>
/// Configuration options for JWT tokens.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether JWT validation is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the authentication scheme name for JWT.
    /// Default is "Bearer".
    /// </summary>
    public string SchemeName { get; set; } = JwtBearerDefaults.AuthenticationScheme;

    /// <summary>
    /// Gets or sets the authentication mode for token validation.
    /// Default is Symmetric (self-issued).
    /// </summary>
    public JwtAuthMode AuthMode { get; set; } = JwtAuthMode.Symmetric;

    /// <summary>
    /// Gets or sets the issuer of the token.
    /// Initializing as string.Empty ensures a strong validation contract by preventing null reference exceptions 
    /// and enforcing that these required configuration values must be explicitly provided or validated.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the intended audience of the token.
    /// Initializing as string.Empty ensures a strong validation contract by preventing null reference exceptions 
    /// and enforcing that these required configuration values must be explicitly provided or validated.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the secret key used to sign the token.
    /// Initializing as string.Empty ensures a strong validation contract by preventing null reference exceptions 
    /// and enforcing that these required configuration values must be explicitly provided or validated.
    /// Required only when AuthMode is Symmetric.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authority URL (OIDC Discovery Base URL).
    /// Required when AuthMode is OidcDiscovery.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the custom metadata address for OIDC discovery, if it cannot be inferred from Authority.
    /// </summary>
    public string? MetadataAddress { get; set; }

    /// <summary>
    /// Gets or sets the expiration time of the access token in minutes.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the clock skew in seconds allowed when validating a token.
    /// </summary>
    public double ClockSkewSeconds { get; set; }

    /// <summary>
    /// Gets or sets the lifetime of a refresh token in days. Defaults to 30.
    /// </summary>
    public int RefreshTokenLifetimeDays { get; set; } = 30;
}
