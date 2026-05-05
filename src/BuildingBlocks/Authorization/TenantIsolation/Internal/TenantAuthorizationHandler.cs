using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.TenantIsolation.Internal;

/// <summary>
/// Enforces tenant-level isolation by comparing the user's tenant claim
/// against the requirement's target tenant.
/// Also provides programmatic evaluation via <see cref="IVKTenantEvaluator"/>.
/// </summary>
internal sealed class TenantAuthorizationHandler(
    IVKUserTenantProvider tenantProvider,
    IOptions<VKAuthorizationOptions> globalOptions,
    IOptions<VKTenantIsolationOptions> tenantOptions,
    ILogger<TenantAuthorizationHandler> logger)
    : AuthorizationHandler<VKTenantIsolationRequirement>, IVKTenantEvaluator
{
    private static string PolicyName => TenantIsolationConstants.FeatureName;

    private readonly IVKUserTenantProvider _tenantProvider = VKGuard.NotNull(tenantProvider);
    private readonly VKAuthorizationOptions _globalOptions = VKGuard.NotNull(globalOptions).Value;
    private readonly VKTenantIsolationOptions _tenantOptions = VKGuard.NotNull(tenantOptions).Value;
    private readonly ILogger<TenantAuthorizationHandler> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VKTenantIsolationRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var targetTenantId = context.Resource as string;
        var result = await HasSameTenantAsync(context.User, new VKTenantIsolationArgs { TargetTenantId = targetTenantId }).ConfigureAwait(false);

        context.ApplyResult(requirement, result, this);
    }

    /// <inheritdoc />
    public ValueTask<VKResult<bool>> HasSameTenantAsync(
        ClaimsPrincipal user,
        VKTenantIsolationArgs? args = null,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(user);
        var userId = user.Identity?.Name ?? VKBlocksConstants.UnknownIdentity;

        // 0. Merge settings (Rule 21)
        var targetTenantId = args.MergeWith(VKTenantIsolationArgs.Empty).TargetTenantId;

        // 1. SuperAdmin Bypass Logic (Centralized via extension)
        if (!_tenantOptions.StrictTenantIsolation && user.IsSuperAdmin(_globalOptions))
        {
            _logger.LogTenantCheckSucceeded(userId, "GLOBAL", targetTenantId ?? "ANY", PolicyName);
            return ValueTask.FromResult(VKResult.Success(true));
        }


        var sw = Stopwatch.StartNew();
        var userTenantId = _tenantProvider.GetUserTenantId(user);

        // 2. Logic Evaluation
        var isAllowed = !string.IsNullOrEmpty(userTenantId) &&
                        (string.IsNullOrEmpty(targetTenantId) ||
                         string.Equals(userTenantId, targetTenantId, StringComparison.OrdinalIgnoreCase));

        // 3. Diagnostics & Recording (Centralized via extension)
        sw.RecordEvaluation(PolicyName, VKResult.Success(isAllowed));

        if (isAllowed)
        {
            _logger.LogTenantCheckSucceeded(userId, userTenantId!, targetTenantId, PolicyName);
            return ValueTask.FromResult(VKResult.Success(true));
        }

        if (string.IsNullOrEmpty(userTenantId))
        {
            _logger.LogTenantCheckMissingId(userId, PolicyName);
        }
        else
        {
            _logger.LogTenantCheckMismatch(userId, userTenantId, targetTenantId, PolicyName);
        }


        return ValueTask.FromResult(VKResult.Success(false));
    }
}
