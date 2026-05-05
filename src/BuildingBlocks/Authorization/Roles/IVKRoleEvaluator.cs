using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Provides programmatic role evaluation with VKResult and telemetry support.
/// </summary>
public interface IVKRoleEvaluator : IVKEvaluator<VKRoleArgs>
{
    /// <summary>
    /// Checks if a user has a specific role asynchronously.
    /// </summary>
    ValueTask<VKResult<bool>> HasRoleAsync(ClaimsPrincipal user, string role, CancellationToken ct = default)
        => HasRolesAsync(user, new VKRoleArgs { Roles = [role] }, ct);

    /// <summary>
    /// Checks if a user has the specified roles asynchronously.
    /// </summary>
    ValueTask<VKResult<bool>> HasRolesAsync(ClaimsPrincipal user, VKRoleArgs? args = null, CancellationToken ct = default);

    /// <inheritdoc />
    ValueTask<VKResult<bool>> IVKEvaluator<VKRoleArgs>.EvaluateAsync(
        ClaimsPrincipal user,
        VKRoleArgs? args,
        CancellationToken ct) => HasRolesAsync(user, args, ct);
}
