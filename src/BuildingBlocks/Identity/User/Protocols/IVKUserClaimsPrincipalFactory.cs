using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain-to-Runtime translation contract converting domain user and memberships into ClaimsPrincipal.
/// </summary>
public interface IVKUserClaimsPrincipalFactory
{
    /// <summary>
    /// Creates a <see cref="ClaimsPrincipal"/> for the specified user in the active tenant context.
    /// </summary>
    Task<VKResult<ClaimsPrincipal>> CreateAsync(
        VKUser user,
        VKTenantId tenantId,
        CancellationToken ct = default);
}
