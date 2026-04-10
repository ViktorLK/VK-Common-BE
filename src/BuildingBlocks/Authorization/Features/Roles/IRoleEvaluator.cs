using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Roles;

/// <summary>
/// Provides programmatic role evaluation with Result and telemetry support.
/// </summary>
public interface IRoleEvaluator
{
    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    ValueTask<Result<bool>> HasRoleAsync(ClaimsPrincipal user, string role, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user has at least one of the specified roles.
    /// </summary>
    ValueTask<Result<bool>> HasRolesAsync(ClaimsPrincipal user, string[] roles, CancellationToken ct = default);
}
