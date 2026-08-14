using System;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Configuration options for the timeout feature slice.
/// </summary>
public sealed partial record VKTimeoutOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the timeout duration for operations.
    /// </summary>
    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(30);
}
