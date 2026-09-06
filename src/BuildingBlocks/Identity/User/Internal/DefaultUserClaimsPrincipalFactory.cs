using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Identity.User.Internal;

/// <summary>
/// Default implementation of <see cref="IVKUserClaimsPrincipalFactory"/> creating claims based on User and Tenant relations.
/// Follows AP.01, CS.01, and CS.03.
/// </summary>
internal sealed class DefaultUserClaimsPrincipalFactory(
    IVKIdentityTenantUserRepository tenantUserRepository) : IVKUserClaimsPrincipalFactory
{
    private readonly IVKIdentityTenantUserRepository _tenantUserRepository = VKGuard.NotNull(tenantUserRepository);

    /// <inheritdoc />
    public async Task<VKResult<ClaimsPrincipal>> CreateAsync(
        VKUser user,
        VKTenantId tenantId,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(user);

        var claims = new List<Claim>
        {
            new(VKClaimConstants.UserId, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email.Value),
            new(VKClaimConstants.TenantId, tenantId.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.DisplayName));
        }

        // Fetch tenant user role if tenant is set
        if (tenantId != VKTenantId.Default)
        {
            var userResult = await _tenantUserRepository.FindAsync(tenantId, user.Id, ct).ConfigureAwait(false);
            if (userResult.IsSuccess && userResult.Value is not null)
            {
                var roleName = userResult.Value.Role.ToString();
                claims.Add(new Claim(VKClaimConstants.Role, roleName));
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }
        }

        var identity = new ClaimsIdentity(claims, "VKIdentityScheme", ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);

        return VKResult.Success(principal);
    }
}
