using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Authentication.Claims;

/// <summary>
/// Intercepts the ClaimsPrincipal after authentication to enrich it with permissions, tenant IDs, etc.
/// It uses <see cref="IVKClaimsProvider"/> if registered in the DI container.
/// </summary>
public class VKClaimsTransformer(IServiceScopeFactory scopeFactory) : IClaimsTransformation
{
    #region Public Methods

    /// <inheritdoc />
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return principal;
        }

        // Only transform if it hasn't mapped yet, avoiding duplicate claims.
        // We use VKClaimTypes.Permissions as an indicator, but this can be adjusted.
        if (principal.HasClaim(c => c.Type == VKClaimTypes.Permissions))
        {
            return principal;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst(VKClaimTypes.UserId)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return principal;
        }

        using var scope = scopeFactory.CreateScope();
        var claimsProvider = scope.ServiceProvider.GetService<IVKClaimsProvider>();

        if (claimsProvider != null)
        {
            var dynamicClaims = await claimsProvider.GetUserClaimsAsync(userId, CancellationToken.None);

            if (dynamicClaims != null)
            {
                identity.AddClaims(dynamicClaims);
            }
        }

        return principal;
    }

    #endregion
}
