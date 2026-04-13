using System.Security.Claims;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Features.OAuth.Metadata;
using VK.Blocks.Authentication.Features.OAuth.Internal;
using VK.Blocks.Core.Context;

namespace VK.Blocks.Authentication.Features.OAuth.Mappers;

/// <summary>
/// Implements claims mapping for GitHub OAuth providers.
/// </summary>
[OAuthProvider(OAuthConstants.GitHub)]
public sealed class GitHubClaimsMapper : OAuthClaimsMapperBase
{
    #region Public Methods

    /// <inheritdoc />
    public override IEnumerable<Claim> MapToClaims(ExternalIdentity userInfo)
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

    #endregion
}
