using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Roles;

/// <summary>
/// Provides a mechanism to check if a user belongs to a specific role.
/// </summary>
public interface IRoleProvider
{
    /// <summary>
    /// Checks if the specified user has a particular role asynchronously.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="role">The role name to verify.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> indicating if the user has the role.</returns>
    ValueTask<Result<bool>> IsInRoleAsync(ClaimsPrincipal user, string role, CancellationToken ct = default);
}
