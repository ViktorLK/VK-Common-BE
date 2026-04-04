using System.Security.Claims;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Features.OAuth.Mappers;

namespace VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Mappers;

/// <summary>
/// Specialized claims mapper for Google OpenID Connect providers.
/// </summary>
[OAuthProvider(OidcConstants.Google)]
public sealed class GoogleOidcClaimsMapper : OAuthClaimsMapperBase
{
    // Google typically follows standard OIDC claims. 
    // This class is provided as a placeholder for specialized Google logic.
}
