using System;

namespace VK.Blocks.Authorization.WorkingHours.Internal;

/// <summary>
/// Defines constants and default values for the WorkingHours feature.
/// </summary>
internal static class WorkingHoursConstants
{
    /// <summary>
    /// Default start time for working hours (09:00).
    /// </summary>
    internal static readonly TimeOnly DefaultStart = new(9, 0);

    /// <summary>
    /// Default end time for working hours (18:00).
    /// </summary>
    internal static readonly TimeOnly DefaultEnd = new(18, 0);

    /// <summary>
    /// The name of the WorkingHours feature.
    /// </summary>
    internal const string FeatureName = "WorkingHours";
}
