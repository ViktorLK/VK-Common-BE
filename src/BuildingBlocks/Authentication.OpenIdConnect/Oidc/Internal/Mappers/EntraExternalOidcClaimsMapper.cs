using System.Collections.Generic;
using System.Security.Claims;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OpenIdConnect.Oidc.Internal.Mappers;

/// <summary>
/// Specialized claims mapper for Microsoft Entra External ID (CIAM) providers.
/// </summary>
[VKOAuthProvider(OidcConstants.EntraExternal)]
public sealed class EntraExternalOidcClaimsMapper : VKOAuthClaimsMapperBase
{
    /// <inheritdoc />
    public override IEnumerable<Claim> MapToClaims(VKExternalIdentity userInfo)
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
