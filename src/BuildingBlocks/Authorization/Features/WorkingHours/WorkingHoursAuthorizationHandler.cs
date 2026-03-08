using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization.Features.WorkingHours;

/// <summary>
/// Grants access only when the current local time falls within the window
/// defined by <see cref="WorkingHoursRequirement"/>.
/// </summary>
public sealed class WorkingHoursAuthorizationHandler(TimeProvider timeProvider)
    : AuthorizationHandler<WorkingHoursRequirement>
{
    #region Public Methods

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WorkingHoursRequirement requirement)
    {
        var now = TimeOnly.FromDateTime(timeProvider.GetLocalNow().LocalDateTime);

        bool withinWindow = requirement.Start <= requirement.End
            ? now >= requirement.Start && now < requirement.End          // same day
            : now >= requirement.Start || now < requirement.End;         // overnight window

        if (withinWindow)
            context.Succeed(requirement);
        else
            context.Fail(new AuthorizationFailureReason(this,
                $"Access is restricted to {requirement.Start} - {requirement.End} (local time)."));

        return Task.CompletedTask;
    }

    #endregion
}


