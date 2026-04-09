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
using VK.Blocks.Authorization.Diagnostics;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.WorkingHours;

/// <summary>
/// Grants access only when the current local time falls within the window.
/// Supports SuperAdmin bypass.
/// </summary>
public sealed class WorkingHoursAuthorizationHandler(
    TimeProvider timeProvider,
    IWorkingHoursProvider workingHoursProvider,
    IOptions<VKAuthorizationOptions> options,
    ILogger<WorkingHoursAuthorizationHandler> logger)
    : AuthorizationHandler<WorkingHoursRequirement>, IWorkingHoursEvaluator
{
    private const string PolicyName = "WorkingHours";
    private readonly VKAuthorizationOptions _options = options.Value;

    #region Public Methods

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkingHoursRequirement requirement)
    {
        var result = await IsWithinWorkingHoursAsync(
                context.User, 
                requirement.Start, 
                requirement.End)
            .ConfigureAwait(false);

        context.ApplyResult(requirement, result, this);
    }

    /// <inheritdoc />
    public async ValueTask<Result<bool>> IsWithinWorkingHoursAsync(
        ClaimsPrincipal user,
        TimeOnly start,
        TimeOnly end,
        CancellationToken ct = default)
    {
        var userId = user.Identity?.Name ?? "Unknown";

        // 1. SuperAdmin Bypass Logic (Centralized via extension)
        if (user.IsSuperAdmin(_options))
        {
            logger.LogAuthorizationSucceeded(userId, TimeOnly.MinValue, start, end, PolicyName + " (Bypassed)");
            return Result.Success(true);
        }

        var sw = Stopwatch.StartNew();

        // 2. Resolve dynamic hours via provider
        var dynamicHours = await workingHoursProvider.GetWorkingHoursAsync(user, ct).ConfigureAwait(false);
        var activeStart = dynamicHours?.Start ?? start;
        var activeEnd = dynamicHours?.End ?? end;

        // 3. Evaluate
        var now = TimeOnly.FromDateTime(timeProvider.GetLocalNow().LocalDateTime);

        var isAllowed = activeStart <= activeEnd
            ? now >= activeStart && now < activeEnd          // same day
            : now >= activeStart || now < activeEnd;         // overnight window

        // 4. Trace & Record
        sw.RecordEvaluation(PolicyName, Result.Success(isAllowed));

        if (isAllowed)
        {
            logger.LogAuthorizationSucceeded(userId, now, activeStart, activeEnd, PolicyName);
            return Result.Success(true);
        }

        logger.LogAuthorizationFailed(userId, now, activeStart, activeEnd, PolicyName);
        return Result.Success(false);
    }

    #endregion
}
