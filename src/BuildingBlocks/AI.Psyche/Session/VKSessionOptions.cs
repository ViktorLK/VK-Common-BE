using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Options for the Session thread feature in Psyche.
/// Follows BB.05 (Options pattern with sealed record).
/// </summary>
public sealed partial record VKSessionOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Session feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default session mode when none is specified.
    /// </summary>
    public VKSessionMode DefaultMode { get; init; } = VKSessionMode.Isolated;

    /// <summary>
    /// Gets or sets the optional session idle timeout duration before considering a session closed.
    /// </summary>
    public TimeSpan? IdleTimeout { get; init; }
}
