using System.Collections.Generic;
using System.Security.Claims;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Provides a base implementation for OAuth claims mappers.
/// </summary>
public abstract class VKOAuthClaimsMapperBase : IVKOAuthClaimsMapper
{
    /// <inheritdoc />
    public virtual IEnumerable<Claim> MapToClaims(VKExternalIdentity userInfo)
    {
        yield return new Claim(VKClaimConstants.UserId, userInfo.ProviderId);
        yield return new Claim(VKClaimConstants.AuthType, userInfo.Provider);

        // Return optional claims only if they are present in the user info.
        if (!string.IsNullOrEmpty(userInfo.Email))
        {
            yield return new Claim(ClaimTypes.Email, userInfo.Email);
        }

        if (!string.IsNullOrEmpty(userInfo.Name))
        {
            yield return new Claim(VKClaimConstants.Name, userInfo.Name);
        }
    }
}
