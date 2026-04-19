using System.Collections.Generic;
using System.Security.Claims;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Features.OAuth.Mappers;
using VK.Blocks.Authentication.Features.OAuth.Metadata;
using VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Internal;
using VK.Blocks.Core.Constants;

namespace VK.Blocks.Authentication.OpenIdConnect.Features.Oidc.Mappers;

/// <summary>
/// Specialized claims mapper for Microsoft Entra External ID (CIAM) providers.
/// </summary>
[OAuthProvider(OidcConstants.EntraExternal)]
public sealed class EntraExternalOidcClaimsMapper : OAuthClaimsMapperBase
{
    /// <inheritdoc />
    public override IEnumerable<Claim> MapToClaims(ExternalIdentity userInfo)
    {
        foreach (var claim in base.MapToClaims(userInfo))
        {
            yield return claim;
        }

        // CIAM / Entra External often uses 'oid' for the unique object identifier
        if (userInfo.Claims.TryGetValue("oid", out var oid))
        {
            yield return new Claim(VKClaimConstants.ExternalId, oid);
        }

        // Handle tenant identifier if present (tid)
        if (userInfo.Claims.TryGetValue("tid", out var tid))
        {
            yield return new Claim(VKClaimConstants.TenantId, tid);
        }
    }
}

