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
public interface IVKPermissionEvaluator : IVKEvaluator<VKPermissionArgs>
{
    /// <summary>
    /// Evaluates whether the claimant possesses the specified permission asynchronously.
    /// </summary>
    ValueTask<VKResult<bool>> HasPermissionAsync(
        ClaimsPrincipal user,
        string permission,
        CancellationToken ct = default) => HasPermissionsAsync(user, new VKPermissionArgs { Permissions = [permission], Mode = VKPermissionEvaluationMode.All }, ct);

    /// <summary>
    /// Evaluates multiple permissions based on the specified mode across all registered providers.
    /// </summary>
    ValueTask<VKResult<bool>> HasPermissionsAsync(
        ClaimsPrincipal user,
        VKPermissionArgs? args = null,
        CancellationToken ct = default);

    /// <inheritdoc />
    ValueTask<VKResult<bool>> IVKEvaluator<VKPermissionArgs>.EvaluateAsync(
        ClaimsPrincipal user,
        VKPermissionArgs? args,
        CancellationToken ct) => HasPermissionsAsync(user, args, ct);
}
