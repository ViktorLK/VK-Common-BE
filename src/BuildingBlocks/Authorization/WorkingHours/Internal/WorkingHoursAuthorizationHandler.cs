using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.WorkingHours.Internal;

/// <summary>
/// Handles <see cref="VKWorkingHoursRequirement"/> by checking the current time
/// against the configured or provided working hours.
/// Also provides programmatic evaluation via <see cref="IVKWorkingHoursEvaluator"/>.
/// </summary>
internal sealed class WorkingHoursAuthorizationHandler(
    TimeProvider timeProvider,
    IVKWorkingHoursProvider workingHoursProvider,
    IOptions<VKAuthorizationOptions> globalOptions,
    IOptions<VKWorkingHoursOptions> workingHoursOptions,
    ILogger<WorkingHoursAuthorizationHandler> logger)
    : AuthorizationHandler<VKWorkingHoursRequirement>, IVKWorkingHoursEvaluator
{
    private static string PolicyName => WorkingHoursConstants.FeatureName;

    private readonly TimeProvider _timeProvider = VKGuard.NotNull(timeProvider);
    private readonly IVKWorkingHoursProvider _workingHoursProvider = VKGuard.NotNull(workingHoursProvider);
    private readonly VKAuthorizationOptions _globalOptions = VKGuard.NotNull(globalOptions).Value;
    private readonly VKWorkingHoursOptions _workingHoursOptions = VKGuard.NotNull(workingHoursOptions).Value;
    private readonly ILogger<WorkingHoursAuthorizationHandler> _logger = VKGuard.NotNull(logger);

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VKWorkingHoursRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var result = await IsWithinWorkingHoursAsync(
                context.User,
                new VKWorkingHoursArgs { Start = requirement.Start, End = requirement.End })
            .ConfigureAwait(false);

        context.ApplyResult(requirement, result, this);
    }

    /// <inheritdoc />
    public async ValueTask<VKResult<bool>> IsWithinWorkingHoursAsync(
        ClaimsPrincipal user,
        VKWorkingHoursArgs? args = null,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(user);
        var userId = user.Identity?.Name ?? VKBlocksConstants.UnknownIdentity;

        // 0. Merge settings (Rule 21)
        var activeStart = args.MergeWith(VKWorkingHoursArgs.Empty).Start.MergeWith(_workingHoursOptions.WorkStart);
        var activeEnd = args.MergeWith(VKWorkingHoursArgs.Empty).End.MergeWith(_workingHoursOptions.WorkEnd);

        // 1. SuperAdmin Bypass Logic (Centralized via extension)
        if (user.IsSuperAdmin(_globalOptions))
        {
            _logger.LogAuthorizationSucceeded(userId, TimeOnly.MinValue, activeStart, activeEnd, $"{WorkingHoursConstants.FeatureName} (Bypassed)");
            return VKResult.Success(true);
        }


        var sw = Stopwatch.StartNew();

        // 2. Resolve dynamic hours via provider
        var dynamicHours = await _workingHoursProvider.GetWorkingHoursAsync(user, ct).ConfigureAwait(false);
        activeStart = dynamicHours?.Start ?? activeStart;
        activeEnd = dynamicHours?.End ?? activeEnd;

        // 3. Evaluate
        var now = TimeOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);

        var isAllowed = activeStart <= activeEnd
            ? now >= activeStart && now < activeEnd          // same day
            : now >= activeStart || now < activeEnd;         // overnight window

        // 4. Trace & Record
        sw.RecordEvaluation(PolicyName, VKResult.Success(isAllowed));

        if (isAllowed)
        {
            _logger.LogAuthorizationSucceeded(userId, now, activeStart, activeEnd, PolicyName);
            return VKResult.Success(true);
        }

        _logger.LogAuthorizationFailed(userId, now, activeStart, activeEnd, PolicyName);
        return VKResult.Success(false);
    }
}
