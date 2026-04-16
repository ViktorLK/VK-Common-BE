using System.Security.Claims;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Core.Constants;

namespace VK.Blocks.Authentication.Features.OAuth.Mappers;

/// <summary>
/// Provides a base implementation for OAuth claims mappers.
/// </summary>
public abstract class OAuthClaimsMapperBase : IOAuthClaimsMapper
{
    /// <inheritdoc />
    public virtual IEnumerable<Claim> MapToClaims(ExternalIdentity userInfo)
    {
        yield return new Claim(VKClaimTypes.UserId, userInfo.ProviderId);
        yield return new Claim(VKClaimTypes.AuthType, userInfo.Provider);

        // Return optional claims only if they are present in the user info.
        if (!string.IsNullOrEmpty(userInfo.Email))
        {
            yield return new Claim(ClaimTypes.Email, userInfo.Email);
        }

        if (!string.IsNullOrEmpty(userInfo.Name))
        {
            yield return new Claim(VKClaimTypes.Name, userInfo.Name);
        }
    }
}
