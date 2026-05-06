namespace VK.Blocks.Authentication.OpenIdConnect.Oidc.Internal.Mappers;

/// <summary>
/// Specialized claims mapper for Google OpenID Connect providers.
/// </summary>
[VKOAuthProvider(OidcConstants.Google)]
public sealed class GoogleOidcClaimsMapper : VKOAuthClaimsMapperBase
{
    // Google typically follows standard OIDC claims. 
    // This class is provided as a placeholder for specialized Google logic.
}
