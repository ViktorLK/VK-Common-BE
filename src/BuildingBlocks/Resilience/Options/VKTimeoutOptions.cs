using System;

namespace VK.Blocks.Resilience;

/// <summary>
/// Configuration options for timeout strategies.
/// </summary>
public sealed record VKTimeoutOptions
{
    /// <summary>
    /// Gets the timeout duration.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}
