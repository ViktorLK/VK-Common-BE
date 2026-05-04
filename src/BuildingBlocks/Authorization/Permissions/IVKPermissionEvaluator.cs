using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines a specialized contract for evaluating user permissions asynchronously.
/// This interface is specifically designed for programmatic VKPermission checks
/// and adheres to the VK VKResult Pattern.
/// </summary>
public interface IVKPermissionEvaluator
{
    /// <summary>
    /// Evaluates whether the claimant possesses the specified VKPermission asynchronously.
    /// </summary>
    /// <param name="user">The user (claims principal) to evaluate.</param>
    /// <param name="VKPermission">The VKPermission name to check.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="VKResult{T}"/> where <c>T</c> is <c>bool</c>.</returns>
    ValueTask<VKResult<bool>> HasPermissionAsync(
        ClaimsPrincipal user,
        string VKPermission,
        CancellationToken ct = default);
}
