using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Authorization.Diagnostics;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// Evaluates <see cref="PermissionRequirement"/> by delegating to <see cref="IPermissionProvider"/>.
/// </summary>
public sealed class PermissionHandler(
    IPermissionProvider permissionProvider,
    ILogger<PermissionHandler> logger)
    : AuthorizationHandler<PermissionRequirement>, IPermissionEvaluator
{
    #region Public Methods

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var result = await HasPermissionAsync(context.User, requirement.Permission).ConfigureAwait(false);
        context.ApplyResult(requirement, result, this);
    }

    /// <inheritdoc />
    public async ValueTask<Result<bool>> HasPermissionAsync(
        ClaimsPrincipal user,
        string permission,
        CancellationToken ct = default)
    {
        var userId = user.Identity?.Name ?? "Unknown";
        var policyName = $"{PermissionsConstants.PolicyPrefix}{permission}";

        var sw = Stopwatch.StartNew();
        var result = await permissionProvider.HasPermissionAsync(user, permission, ct).ConfigureAwait(false);
        
        // 1. Trace & Record (Centralized via extension)
        sw.RecordEvaluation(policyName, result);

        if (result.IsSuccess && result.Value)
        {
            logger.LogPermissionGranted(permission, userId);
            return Result.Success(true);
        }

        if (!result.IsSuccess)
        {
            logger.LogPermissionCheckError(permission, userId, result.FirstError.Code, result.FirstError.Description);
            return Result.Failure<bool>(result.Errors);
        }

        logger.LogPermissionDenied(permission, userId);
        return Result.Success(false);
    }

    #endregion
}
