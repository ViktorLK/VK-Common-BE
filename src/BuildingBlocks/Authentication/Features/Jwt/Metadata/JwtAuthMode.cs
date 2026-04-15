namespace VK.Blocks.Authentication.Features.Jwt.Metadata;

/// <summary>
/// Defines the authentication modes supported by the JWT infrastructure.
/// </summary>
public enum JwtAuthMode
{
    /// <summary>
    /// Self-issued tokens validated with a symmetric key (e.g., HMAC-SHA256).
    /// </summary>
    Symmetric = 0,

    /// <summary>
    /// Third-party tokens validated via OIDC Discovery (JWKS endpoint).
    /// </summary>
    OidcDiscovery = 1
}
