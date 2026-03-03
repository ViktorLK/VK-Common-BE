using System.Collections.Generic;
using System.Security.Claims;
using VK.Blocks.Authentication.Abstractions.Contracts;
using VK.Blocks.Authentication.Claims;

namespace VK.Blocks.Authentication.OAuth.Mappers;

/// <summary>
/// Implements claims mapping for GitHub OAuth providers.
/// </summary>
public class GitHubClaimsMapper : OAuthClaimsMapperBase
{
    /// <inheritdoc />
    public override IEnumerable<Claim> MapToClaims(OAuthUserInfo userInfo)
    {
        // Yield base claims
        foreach (var claim in base.MapToClaims(userInfo))
        {
            yield return claim;
        }

        if (userInfo.Claims.TryGetValue("avatar_url", out var avatarUrl))
        {
            yield return new Claim(VKClaimTypes.AvatarUrl, avatarUrl);
        }

        if (userInfo.Claims.TryGetValue("html_url", out var htmlUrl) ||
            userInfo.Claims.TryGetValue("url", out htmlUrl))
        {
            yield return new Claim(VKClaimTypes.ProfileUrl, htmlUrl);
        }
    }
}
