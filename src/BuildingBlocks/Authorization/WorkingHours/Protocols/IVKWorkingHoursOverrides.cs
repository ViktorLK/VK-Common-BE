using System;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines request-level overrides and target parameters for Working Hours authorization.
/// </summary>
public interface IVKWorkingHoursOverrides
{
    /// <summary>
    /// Gets the start of the working hours window (local time), overriding the default settings.
    /// </summary>
    TimeOnly? WorkStart { get; init; }

    /// <summary>
    /// Gets the end of the working hours window (local time), overriding the default settings.
    /// </summary>
    TimeOnly? WorkEnd { get; init; }
}
