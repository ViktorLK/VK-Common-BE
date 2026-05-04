using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Provides programmatic role evaluation with VKResult and telemetry support.
/// </summary>
public interface IVKRoleEvaluator
{
    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    ValueTask<VKResult<bool>> HasRoleAsync(ClaimsPrincipal user, string role, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user has at least one of the specified roles.
    /// </summary>
    ValueTask<VKResult<bool>> HasRolesAsync(ClaimsPrincipal user, string[] roles, CancellationToken ct = default);
}
