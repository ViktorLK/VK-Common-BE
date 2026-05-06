namespace VK.Blocks.Authentication.OpenIdConnect.Oidc.Internal.Mappers;

/// <summary>
/// A general-purpose claims mapper that follows standard OIDC claims (e.g. sub, email, name).
/// Used as a fallback for any OIDC provider that doesn't have a specialized mapper.
/// </summary>
[VKOAuthProvider(OidcConstants.StandardProvider)]
public sealed class StandardOidcClaimsMapper : VKOAuthClaimsMapperBase
{
    // No additional mapping logic needed for standard OIDC beyond base class.
}
