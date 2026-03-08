using System;

namespace VK.Blocks.Authorization.Features.WorkingHours;

/// <summary>
/// Requires the request to arrive within a configured local-time window.
/// Use with <c>WorkingHoursAuthorizationHandler</c>.
/// </summary>
/// <param name="Start">Inclusive start of the allowed time window (local time).</param>
/// <param name="End">Exclusive end of the allowed time window (local time).</param>
public sealed record WorkingHoursRequirement : Microsoft.AspNetCore.Authorization.IAuthorizationRequirement
{
    #region Properties

    /// <summary>
    /// Inclusive start of the allowed time window (local time).
    /// </summary>
    public TimeOnly Start { get; init; }

    /// <summary>
    /// Exclusive end of the allowed time window (local time).
    /// </summary>
    public TimeOnly End { get; init; }

    #endregion

    public WorkingHoursRequirement(TimeOnly start, TimeOnly end)
    {
        Start = start;
        End = end;
    }
}


