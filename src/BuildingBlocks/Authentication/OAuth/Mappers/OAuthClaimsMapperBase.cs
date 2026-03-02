using System.Collections.Generic;
using System.Security.Claims;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Abstractions.Contracts;

namespace VK.Blocks.Authentication.OAuth.Mappers;

/// <summary>
/// Provides a base implementation for OAuth claims mappers.
/// </summary>
public abstract class OAuthClaimsMapperBase : IOAuthClaimsMapper
{
    #region Public Methods

    /// <inheritdoc />
    public virtual IEnumerable<Claim> MapToClaims(OAuthUserInfo userInfo)
    {
        yield return new Claim(ClaimTypes.NameIdentifier, userInfo.ProviderId);
        yield return new Claim(ClaimTypes.AuthenticationMethod, userInfo.Provider);

        // Return optional claims only if they are present in the user info.
        if (!string.IsNullOrEmpty(userInfo.Email))
        {
            yield return new Claim(ClaimTypes.Email, userInfo.Email);
        }

        if (!string.IsNullOrEmpty(userInfo.Name))
        {
            yield return new Claim(ClaimTypes.Name, userInfo.Name);
        }
    }

    #endregion
}
