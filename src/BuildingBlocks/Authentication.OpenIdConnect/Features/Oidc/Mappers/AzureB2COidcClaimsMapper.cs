using System.Security.Claims;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Features.OAuth.Mappers;
using VK.Blocks.Authentication.Features.OAuth.Metadata;
using VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Internal;

namespace VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Mappers;

/// <summary>
/// Specialized claims mapper for Azure B2C OpenID Connect providers.
/// </summary>
[OAuthProvider(OidcConstants.AzureB2C)]
public sealed class AzureB2COidcClaimsMapper : OAuthClaimsMapperBase
{
    #region Public Methods

    public override IEnumerable<Claim> MapToClaims(ExternalIdentity userInfo)
    {
        foreach (var claim in base.MapToClaims(userInfo))
        {
            yield return claim;
        }

        if (userInfo.Claims.TryGetValue(OidcConstants.ClaimTfp, out var tfp) ||
            userInfo.Claims.TryGetValue(OidcConstants.ClaimAcr, out tfp))
        {
            yield return new Claim(VKClaimTypes.TrustFrameworkPolicy, tfp);
        }

        if (string.IsNullOrEmpty(userInfo.Email) && userInfo.Claims.TryGetValue(OidcConstants.ClaimEmails, out var emails))
        {
            yield return new Claim(ClaimTypes.Email, emails);
        }
    }

    #endregion
}
