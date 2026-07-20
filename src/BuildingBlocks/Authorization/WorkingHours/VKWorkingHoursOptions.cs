using System;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Working Hours authorization feature.
/// </summary>

public sealed partial record VKWorkingHoursOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the working hours feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the start of the working hours window (local time).
    /// </summary>
    [VKRequestOverride]
    public TimeOnly WorkStart { get; init; } = new(9, 0);

    /// <summary>
    /// Gets the end of the working hours window (local time).
    /// </summary>
    [VKRequestOverride]
    public TimeOnly WorkEnd { get; init; } = new(18, 0);
}
