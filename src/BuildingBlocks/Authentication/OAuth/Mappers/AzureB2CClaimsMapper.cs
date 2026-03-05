using System.Collections.Generic;
using System.Security.Claims;
using VK.Blocks.Authentication.Abstractions.Contracts;
using VK.Blocks.Authentication.Claims;

namespace VK.Blocks.Authentication.OAuth.Mappers;

/// <summary>
/// Implements claims mapping for Azure B2C OAuth providers.
/// </summary>
public class AzureB2CClaimsMapper : OAuthClaimsMapperBase
{
    #region Public Methods

    /// <inheritdoc />
    public override IEnumerable<Claim> MapToClaims(OAuthUserInfo userInfo)
    {
        // Yield base claims
        foreach (var claim in base.MapToClaims(userInfo))
        {
            yield return claim;
        }

        // B2C uses 'tfp' (Trust Framework Policy) or 'acr' (Authentication Context Class Reference)
        // to specify which user flow/custom policy was executed.
        if (userInfo.Claims.TryGetValue("tfp", out var tfp) ||
            userInfo.Claims.TryGetValue("acr", out tfp))
        {
            yield return new Claim(VKClaimTypes.TrustFrameworkPolicy, tfp);
        }
    }

    #endregion
}
