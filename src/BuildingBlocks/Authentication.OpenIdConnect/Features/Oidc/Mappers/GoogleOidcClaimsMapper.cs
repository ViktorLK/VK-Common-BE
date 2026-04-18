using VK.Blocks.Authentication.Features.OAuth.Mappers;
using VK.Blocks.Authentication.Features.OAuth.Metadata;
using VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Internal;

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
