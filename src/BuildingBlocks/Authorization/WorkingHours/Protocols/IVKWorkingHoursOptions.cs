using System;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines global static options for Working Hours authorization.
/// </summary>
public interface IVKWorkingHoursOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the start of the working hours window (local time).
    /// </summary>
    TimeOnly WorkStart { get; init; }

    /// <summary>
    /// Gets the end of the working hours window (local time).
    /// </summary>
    TimeOnly WorkEnd { get; init; }
}
