using System;

using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Arguments for working hours evaluation.
/// Following AP.05: Local overrides for the global <see cref="VKWorkingHoursOptions"/>.
/// </summary>
public sealed record VKWorkingHoursArgs : IVKArgs<VKWorkingHoursArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKWorkingHoursArgs Empty { get; } = new();

    /// <summary>
    /// Gets the start of the working hours window.
    /// If null, the value from global options is used.
    /// </summary>
    public TimeOnly? Start { get; init; }

    /// <summary>
    /// Gets the end of the working hours window.
    /// If null, the value from global options is used.
    /// </summary>
    public TimeOnly? End { get; init; }
}

