using System;
using VK.Blocks.Authorization.WorkingHours.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Configuration options for the Working Hours authorization feature.
/// </summary>
public sealed record VKWorkingHoursOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAuthorizationBlock.BlockName}:{WorkingHoursConstants.FeatureName}";

    /// <summary>
    /// Gets a value indicating whether the working hours feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the start of the working hours window (local time).
    /// </summary>
    public TimeOnly WorkStart { get; init; } = new(9, 0);

    /// <summary>
    /// Gets the end of the working hours window (local time).
    /// </summary>
    public TimeOnly WorkEnd { get; init; } = new(18, 0);
}
