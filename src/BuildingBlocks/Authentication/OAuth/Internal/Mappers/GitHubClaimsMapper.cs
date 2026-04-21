using System.Collections.Generic;
using System.Security.Claims;

using VK.Blocks.Core;

namespace VK.Blocks.Authentication.OAuth.Internal.Mappers;

/// <summary>
/// Implements claims mapping for GitHub OAuth providers.
/// </summary>
[OAuthProvider(OAuthConstants.GitHub)]
internal sealed class GitHubClaimsMapper : VKOAuthClaimsMapperBase
{
    /// <inheritdoc />
    public override IEnumerable<Claim> MapToClaims(VKExternalIdentity userInfo)
    {
        // Yield base claims
        foreach (Claim claim in base.MapToClaims(userInfo))
        {
            yield return claim;
        }

        if (userInfo.Claims.TryGetValue("avatar_url", out string? avatarUrl))
        {
            yield return new Claim(VKClaimConstants.AvatarUrl, avatarUrl);
        }

        if (userInfo.Claims.TryGetValue("html_url", out string? htmlUrl) ||
            userInfo.Claims.TryGetValue("url", out htmlUrl))
        {
            yield return new Claim(VKClaimConstants.ProfileUrl, htmlUrl);
        }
    }
}







