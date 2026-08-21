using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Root options for VK.Blocks.AI.Cortex module.
/// Follows [BB.05] Options Architecture (sealed partial record + IVKToggleableBlockOptions).
/// </summary>
public sealed partial record VKAICortexOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the AI.Cortex block is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the default idle timeout threshold after which a session is considered inactive.
    /// </summary>
    public TimeSpan DefaultSessionIdleTimeout { get; init; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Gets a value indicating whether cross-day boundary triggers automatic session consolidation.
    /// </summary>
    public bool EnableCrossDaySessionBoundary { get; init; } = true;
}
