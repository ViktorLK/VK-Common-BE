using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Roles.Internal;

/// <summary>
/// Evaluates <see cref="VKRoleRequirement"/> against the user's claims.
/// Also provides programmatic evaluation via <see cref="IVKRoleEvaluator"/>.
/// </summary>
internal sealed class RoleHandler(
    IEnumerable<IVKRoleProvider> roleProviders,
    IOptions<VKAuthorizationOptions> globalOptions,
    ILogger<RoleHandler> logger)
    : AuthorizationHandler<VKRoleRequirement>, IVKRoleEvaluator
{
    private readonly List<IVKRoleProvider> _providers = [.. VKGuard.NotNull(roleProviders)];
    private readonly VKAuthorizationOptions _globalOptions = VKGuard.NotNull(globalOptions).Value;

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VKRoleRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var result = await HasRolesAsync(context.User, new VKRoleArgs { Roles = requirement.Roles }).ConfigureAwait(false);
        context.ApplyResult(requirement, result, this);
    }

    /// <inheritdoc />
    public async ValueTask<VKResult<bool>> HasRoleAsync(
        ClaimsPrincipal user,
        string role,
        CancellationToken ct = default)
    {
        return await HasRolesAsync(user, new VKRoleArgs { Roles = [role] }, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<VKResult<bool>> HasRolesAsync(
        ClaimsPrincipal user,
        VKRoleArgs? args = null,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(user);
        var userId = user.Identity?.Name ?? VKBlocksConstants.UnknownIdentity;

        // 0. Merge settings (Rule 21)
        var roles = args.MergeWith(VKRoleArgs.Empty).Roles.MergeWith([]);
        if (roles.Length == 0)
        {
            return VKResult.Success(false);
        }

        // 1. SuperAdmin Bypass Logic
        if (user.IsSuperAdmin(_globalOptions))
        {
            return VKResult.Success(true);
        }

        var requiredRolesStr = string.Join(", ", roles);
        var policyName = roles.Length == 1
            ? $"{RolesConstants.PolicyPrefix}:{roles[0]}"
            : $"{RolesConstants.FeatureName}[{requiredRolesStr}]";

        var sw = Stopwatch.StartNew();
        var isAllowed = false;
        string? matchedRole = null;
        VKError? lastError = null;

        foreach (var role in roles)
        {
            var isThisRoleAllowed = false;
            foreach (var provider in _providers)
            {
                var result = await provider.IsInRoleAsync(user, role, ct).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    if (result.Value)
                    {
                        isThisRoleAllowed = true;
                        break;
                    }
                }
                else
                {
                    lastError = result.FirstError;
                }
            }

            if (isThisRoleAllowed)
            {
                isAllowed = true;
                matchedRole = role;
                break;
            }
        }

        // 1. Trace & Record (Centralized via extension)
        var finalResult = lastError is not null ? VKResult.Failure<bool>(lastError) : VKResult.Success(isAllowed);
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
