using System.Collections.Generic;
using System.Security.Claims;
using VK.Blocks.Authentication.Abstractions.Contracts;
using VK.Blocks.Authentication.Claims;

namespace VK.Blocks.Authentication.OAuth.Mappers;

/// <summary>
/// Implements claims mapping for Google OAuth providers.
/// </summary>
public class GoogleClaimsMapper : OAuthClaimsMapperBase
{
    /// <inheritdoc />
    public override IEnumerable<Claim> MapToClaims(OAuthUserInfo userInfo)
    {
        // Yield base claims
        foreach (var claim in base.MapToClaims(userInfo))
        {
            yield return claim;
        }

        if (userInfo.Claims.TryGetValue("picture", out var picture))
        {
            yield return new Claim(VKClaimTypes.AvatarUrl, picture);
        }

        if (userInfo.Claims.TryGetValue("locale", out var locale))
        {
            yield return new Claim(VKClaimTypes.Locale, locale);
        }
    }
}
