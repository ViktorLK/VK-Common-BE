using System;

namespace VK.Blocks.Authorization.Features.WorkingHours;

/// <summary>
/// Defines constants and default values for the WorkingHours feature.
/// </summary>
public static class WorkingHoursConstants
{
    /// <summary>
    /// Default start time for working hours (09:00).
    /// </summary>
    public static readonly TimeOnly DefaultStart = new(9, 0);

    /// <summary>
    /// Default end time for working hours (18:00).
    /// </summary>
    public static readonly TimeOnly DefaultEnd = new(18, 0);
}
