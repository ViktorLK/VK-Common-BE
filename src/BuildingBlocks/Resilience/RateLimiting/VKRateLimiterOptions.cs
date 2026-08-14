using System;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Configuration options for the rate limiting throughput slice.
/// </summary>
public sealed partial record VKRateLimiterOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the maximum number of requests allowed per time window.
    /// </summary>
    public int PermitLimit { get; init; } = 100;

    /// <summary>
    /// Gets the time window duration for the permit limit.
    /// </summary>
    public TimeSpan Window { get; init; } = TimeSpan.FromMinutes(1);
}
