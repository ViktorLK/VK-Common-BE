using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace VK.Blocks.Authentication.Claims;

/// <summary>
/// Intercepts the ClaimsPrincipal after authentication to enrich it with permissions, tenant IDs, etc.
/// </summary>
public class VKClaimsTransformer : IClaimsTransformation
{
    // A service locator can be injected if we need DB access
    // private readonly IServiceScopeFactory _scopeFactory;

    // public VKClaimsTransformer(IServiceScopeFactory scopeFactory)
    // {
    //     _scopeFactory = scopeFactory;
    // }

    #region Public Methods

    /// <inheritdoc />
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
        {
            return Task.FromResult(principal);
        }

        // Example: Only transform if it hasn't mapped yet
        if (!principal.HasClaim(c => c.Type == VKClaimTypes.Permissions))
        {
            // Simulate reading from DB via scope
            // using var scope = _scopeFactory.CreateScope();
            // var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
            // var permissions = await permissionService.GetForUserAsync(userId);

            // Add custom claims natively
            identity.AddClaim(new Claim(VKClaimTypes.Permissions, "read:users"));
            identity.AddClaim(new Claim(VKClaimTypes.Permissions, "write:users"));
            identity.AddClaim(new Claim(VKClaimTypes.TenantId, "tenant-1"));
        }

        return Task.FromResult(principal);
    }

    #endregion
}
