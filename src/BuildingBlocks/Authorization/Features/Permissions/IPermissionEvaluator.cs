using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// Defines a specialized contract for evaluating user permissions asynchronously.
/// This interface is specifically designed for programmatic permission checks
/// and adheres to the VK Result Pattern.
/// </summary>
public interface IPermissionEvaluator
{
    /// <summary>
    /// Evaluates whether the claimant possesses the specified permission asynchronously.
    /// </summary>
    /// <param name="user">The user (claims principal) to evaluate.</param>
    /// <param name="permission">The permission name to check.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="Result{T}"/> where <c>T</c> is <c>bool</c>.</returns>
    ValueTask<Result<bool>> HasPermissionAsync(
        ClaimsPrincipal user, 
        string permission, 
        CancellationToken ct = default);
}
