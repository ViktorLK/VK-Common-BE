using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Permissions.Internal;

/// <summary>
/// Evaluates <see cref="VKPermissionRequirement"/> against the user's claims.
/// Also provides programmatic evaluation via <see cref="IVKPermissionEvaluator"/>.
/// </summary>
internal sealed class PermissionHandler(
    IEnumerable<IVKPermissionProvider> permissionProviders,
    IOptions<VKAuthorizationOptions> globalOptions,
    ILogger<PermissionHandler> logger)
    : AuthorizationHandler<VKPermissionRequirement>, IVKPermissionEvaluator
{
    private readonly List<IVKPermissionProvider> _providers = [.. VKGuard.NotNull(permissionProviders)];
    private readonly VKAuthorizationOptions _globalOptions = VKGuard.NotNull(globalOptions).Value;

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VKPermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var result = await HasPermissionsAsync(context.User, requirement.Permissions, requirement.Mode).ConfigureAwait(false);
        context.ApplyResult(requirement, result, this);
    }

    /// <summary>
    /// Evaluates multiple permissions based on the specified mode across all registered providers.
    /// </summary>
    /// <param name="user">The user to evaluate.</param>
    /// <param name="permissions">The collection of permissions to check.</param>
    /// <param name="mode">The evaluation mode (All/Any).</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result indicating if the permissions are granted.</returns>
    public async ValueTask<VKResult<bool>> HasPermissionsAsync(
        ClaimsPrincipal user,
        IEnumerable<string> permissions,
        VKPermissionEvaluationMode mode,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(user);
        var permissionList = VKGuard.NotNull(permissions).ToList();
        var userId = user.Identity?.Name ?? VKBlocksConstants.UnknownIdentity;

        // 1. SuperAdmin Bypass Logic (Centralized via extension)
        if (user.IsSuperAdmin(_globalOptions))
        {
            foreach (var perm in permissionList)
            {
                logger.LogPermissionGranted(perm, $"{userId} (Bypassed)");
            }
            return VKResult.Success(true);
        }

        var compositePolicy = mode == VKPermissionEvaluationMode.All
            ? $"{PermissionsConstants.FeatureName}:All[{string.Join(",", permissionList)}]"
            : $"{PermissionsConstants.FeatureName}:Any[{string.Join(",", permissionList)}]";

        var sw = Stopwatch.StartNew();

        var isAllowed = mode == VKPermissionEvaluationMode.All;
        VKError? lastError = null;

        foreach (var permission in permissionList)
        {
            var isThisPermissionAllowed = false;

            // Check across all providers (OR logic: any provider granting counts as allowed)
            foreach (var provider in _providers)
            {
                var result = await provider.HasPermissionAsync(user, permission, ct).ConfigureAwait(false);

                if (!result.IsSuccess)
                {
                    lastError = result.FirstError;
                    logger.LogPermissionCheckError(permission, userId, lastError.Code, lastError.Description);
                    continue; // Check next provider
                }

                if (result.Value)
                {
                    isThisPermissionAllowed = true;
                    break; // Granted by this provider, no need to check others for THIS permission
                }
            }

            if (isThisPermissionAllowed)
            {
                logger.LogPermissionGranted(permission, userId);
                if (mode == VKPermissionEvaluationMode.Any)
                {
                    isAllowed = true;
                    break;
                }
            }
            else
            {
                logger.LogPermissionDenied(permission, userId);
                if (mode == VKPermissionEvaluationMode.All)
                {
                    isAllowed = false;
                    break;
                }
            }
        }

        var finalResult = lastError is not null && !isAllowed
            ? VKResult.Failure<bool>(lastError)
            : VKResult.Success(isAllowed);

        // Record aggregated evaluation
        sw.RecordEvaluation(compositePolicy, finalResult);

        return finalResult;
    }

    /// <inheritdoc />
    public ValueTask<VKResult<bool>> HasPermissionAsync(
        ClaimsPrincipal user,
        string permission,
        CancellationToken ct = default)
    {
        return HasPermissionsAsync(user, [permission], VKPermissionEvaluationMode.All, ct);
    }
}
