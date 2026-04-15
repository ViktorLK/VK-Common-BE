using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Authorization.Diagnostics;
using VK.Blocks.Authorization.Features.Permissions.Internal;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// Evaluates <see cref="PermissionRequirement"/> by delegating to multiple <see cref="IPermissionProvider"/> implementations.
/// </summary>
public sealed class PermissionHandler(
    IEnumerable<IPermissionProvider> permissionProviders,
    ILogger<PermissionHandler> logger)
    : AuthorizationHandler<PermissionRequirement>, IPermissionEvaluator
{
    private readonly List<IPermissionProvider> _providers = [.. permissionProviders];

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
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
    public async ValueTask<Result<bool>> HasPermissionsAsync(
        ClaimsPrincipal user,
        IEnumerable<string> permissions,
        PermissionEvaluationMode mode,
        CancellationToken ct = default)
    {
        var permissionList = permissions.ToList();
        var userId = user.Identity?.Name ?? "Unknown";
        var compositePolicy = mode == PermissionEvaluationMode.All
            ? $"Permissions:All[{string.Join(",", permissionList)}]"
            : $"Permissions:Any[{string.Join(",", permissionList)}]";

        var sw = Stopwatch.StartNew();

        var isAllowed = mode == PermissionEvaluationMode.All;
        Error? lastError = null;

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
                if (mode == PermissionEvaluationMode.Any)
                {
                    isAllowed = true;
                    break;
                }
            }
            else
            {
                logger.LogPermissionDenied(permission, userId);
                if (mode == PermissionEvaluationMode.All)
                {
                    isAllowed = false;
                    break;
                }
            }
        }

        var finalResult = lastError is not null && !isAllowed
            ? Result.Failure<bool>(lastError)
            : Result.Success(isAllowed);

        // Record aggregated evaluation
        sw.RecordEvaluation(compositePolicy, finalResult);

        return finalResult;
    }

    /// <inheritdoc />
    public ValueTask<Result<bool>> HasPermissionAsync(
        ClaimsPrincipal user,
        string permission,
        CancellationToken ct = default)
    {
        return HasPermissionsAsync(user, [permission], PermissionEvaluationMode.All, ct);
    }
}
