using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Provides a mechanism to check if a user belongs to a specific role.
/// </summary>
public interface IVKRoleProvider
{
    /// <summary>
    /// Checks if the specified user has a particular role asynchronously.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="role">The role name to verify.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> indicating if the user has the role.</returns>
    ValueTask<VKResult<bool>> IsInRoleAsync(ClaimsPrincipal user, string role, CancellationToken ct = default);
}
