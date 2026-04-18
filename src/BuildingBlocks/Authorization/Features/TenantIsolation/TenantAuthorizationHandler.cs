using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Authorization.DependencyInjection;
using VK.Blocks.Authorization.Features.TenantIsolation.Internal;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.TenantIsolation;

/// <summary>
/// Enforces tenant isolation and supports configurable bypass for SuperAdmins.
/// </summary>
public sealed class TenantAuthorizationHandler(
    IUserTenantProvider userTenantProvider,
    IOptions<VKAuthorizationOptions> options,
    ILogger<TenantAuthorizationHandler> logger)
    : AuthorizationHandler<SameTenantRequirement>, ITenantEvaluator
{
    private const string PolicyName = "TenantIsolation";

    private readonly VKAuthorizationOptions _options = options.Value;

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameTenantRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var targetTenantId = context.Resource as string;
        var result = await HasSameTenantAsync(context.User, targetTenantId).ConfigureAwait(false);

        context.ApplyResult(requirement, result, this);
    }

    /// <inheritdoc />
    public ValueTask<Result<bool>> HasSameTenantAsync(
        ClaimsPrincipal user,
        string? targetTenantId = null,
        CancellationToken ct = default)
    {
        var userId = user.Identity?.Name ?? "Unknown";

        // 1. SuperAdmin Bypass Logic (Centralized via extension)
        if (!_options.StrictTenantIsolation && user.IsSuperAdmin(_options))
        {
            logger.LogTenantCheckSucceeded(userId, "GLOBAL", targetTenantId ?? "ANY", PolicyName);
            return ValueTask.FromResult(Result.Success(true));
        }

        var sw = Stopwatch.StartNew();
        var userTenantId = userTenantProvider.GetUserTenantId(user);

        // 2. Logic Evaluation
        var isAllowed = !string.IsNullOrEmpty(userTenantId) &&
                        (string.IsNullOrEmpty(targetTenantId) ||
                         string.Equals(userTenantId, targetTenantId, StringComparison.OrdinalIgnoreCase));

        // 3. Diagnostics & Recording (Centralized via extension)
        sw.RecordEvaluation(PolicyName, Result.Success(isAllowed));

        if (isAllowed)
        {
            logger.LogTenantCheckSucceeded(userId, userTenantId!, targetTenantId, PolicyName);
            return ValueTask.FromResult(Result.Success(true));
        }

        if (string.IsNullOrEmpty(userTenantId))
        {
            logger.LogTenantCheckMissingId(userId, PolicyName);
        }
        else
        {
            logger.LogTenantCheckMismatch(userId, userTenantId, targetTenantId, PolicyName);
        }

        return ValueTask.FromResult(Result.Success(false));
    }
}
