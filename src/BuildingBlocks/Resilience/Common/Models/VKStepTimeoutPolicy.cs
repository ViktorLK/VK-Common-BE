using System;

namespace VK.Blocks.Resilience;

/// <summary>
/// Step-level timeout policy defining execution duration limits for external calls.
/// Follows [AP.01] and [BB.05].
/// </summary>
public sealed record VKStepTimeoutPolicy
{
    /// <summary>
    /// Gets the timeout duration for the external operation.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Creates a timeout policy with the specified duration.
    /// </summary>
    public static VKStepTimeoutPolicy FromSeconds(double seconds) => new() { Timeout = TimeSpan.FromSeconds(seconds) };

    /// <summary>
    /// Creates a timeout policy with the specified duration.
    /// </summary>
    public static VKStepTimeoutPolicy FromTimeSpan(TimeSpan timeout) => new() { Timeout = timeout };
}
