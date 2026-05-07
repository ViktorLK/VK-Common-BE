using System.Collections.Generic;
using System.Security.Claims;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect.Oidc.Internal.Mappers;

/// <summary>
/// Specialized claims mapper for Azure B2C OpenID Connect providers.
/// </summary>
[VKOAuthProvider(OidcConstants.AzureB2C)]
public sealed class AzureB2COidcClaimsMapper : VKOAuthClaimsMapperBase
{
    /// <inheritdoc />
    public override IEnumerable<Claim> MapToClaims(VKExternalIdentity userInfo)
    {
        foreach (var claim in base.MapToClaims(userInfo))
        {
            yield return claim;
        }

        if (userInfo.Claims.TryGetValue(OidcConstants.ClaimTfp, out var tfp) ||
            userInfo.Claims.TryGetValue(OidcConstants.ClaimAcr, out tfp))
        {
            yield return new Claim(VKClaimConstants.TrustFrameworkPolicy, tfp);
        }

        if (string.IsNullOrEmpty(userInfo.Email) && userInfo.Claims.TryGetValue(OidcConstants.ClaimEmails, out var emails))
        {
            yield return new Claim(ClaimTypes.Email, emails);
        }
    }
}
