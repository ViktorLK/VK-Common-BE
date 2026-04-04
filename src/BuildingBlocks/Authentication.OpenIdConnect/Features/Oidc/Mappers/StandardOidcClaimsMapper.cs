using System.Security.Claims;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Features.OAuth.Mappers;

namespace VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Mappers;

/// <summary>
/// A general-purpose claims mapper that follows standard OIDC claims (e.g. sub, email, name).
/// Used as a fallback for any OIDC provider that doesn't have a specialized mapper.
/// </summary>
[OAuthProvider(OidcConstants.StandardProvider)]
public sealed class StandardOidcClaimsMapper : OAuthClaimsMapperBase
{
    // No additional mapping logic needed for standard OIDC beyond base class.
}
