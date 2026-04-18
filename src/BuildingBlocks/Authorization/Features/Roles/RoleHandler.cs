using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Authorization.Features.Roles.Internal;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Roles;

/// <summary>
/// Evaluates <see cref="RoleRequirement"/> by checking the user's role claims via an <see cref="IRoleProvider"/>.
/// </summary>
public sealed class RoleHandler(
    IRoleProvider roleProvider,
    ILogger<RoleHandler> logger)
    : AuthorizationHandler<RoleRequirement>, IRoleEvaluator
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RoleRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var result = await HasRolesAsync(context.User, requirement.Roles).ConfigureAwait(false);
        context.ApplyResult(requirement, result, this);
    }

    /// <inheritdoc />
    public async ValueTask<Result<bool>> HasRoleAsync(
        ClaimsPrincipal user,
        string role,
        CancellationToken ct = default)
    {
        return await HasRolesAsync(user, [role], ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<Result<bool>> HasRolesAsync(
        ClaimsPrincipal user,
        string[] roles,
        CancellationToken ct = default)
    {
        var userId = user.Identity?.Name ?? "Unknown";
        var requiredRolesStr = string.Join(", ", roles);
        var policyName = roles.Length == 1
            ? $"{RolesConstants.PolicyPrefix}{roles[0]}"
            : $"{RolesConstants.MultiRolePrefix}[{requiredRolesStr}]";

        var sw = Stopwatch.StartNew();
        var isAllowed = false;
        string? matchedRole = null;
        Error? lastError = null;

        foreach (var role in roles)
        {
            var result = await roleProvider.IsInRoleAsync(user, role, ct).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                if (result.Value)
                {
                    isAllowed = true;
                    matchedRole = role;
                    break;
                }
            }
            else
            {
                lastError = result.FirstError;
            }
        }

        // 1. Trace & Record (Centralized via extension)
        var finalResult = lastError is not null ? Result.Failure<bool>(lastError) : Result.Success(isAllowed);
        sw.RecordEvaluation(policyName, finalResult);

        if (finalResult.IsSuccess && finalResult.Value)
        {
            if (matchedRole != null)
            {
                logger.LogRoleGranted(matchedRole, userId);
            }
        }
        else
        {
            logger.LogRolesDenied(userId, requiredRolesStr);
        }

        return finalResult;
    }
}
