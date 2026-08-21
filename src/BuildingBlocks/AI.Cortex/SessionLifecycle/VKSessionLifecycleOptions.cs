using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Options for Session Lifecycle slice.
/// Follows BB.05.
/// </summary>
public sealed partial record VKSessionLifecycleOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether Session Lifecycle coordination is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the idle duration before declaring a session expired.
    /// </summary>
    public TimeSpan IdleTimeout { get; init; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets a value indicating whether crossing UTC calendar day marks the session as boundary-ended.
    /// </summary>
    public bool EnableCrossDayBoundary { get; init; } = true;
}
