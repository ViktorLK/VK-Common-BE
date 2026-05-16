using System;

namespace VK.Blocks.AI;

/// <summary>
/// Defines resilience parameters that can be overridden at the request level.
/// [AP.05] Excludes system-level static configurations like Circuit Breaker thresholds.
/// </summary>
public interface IVKAIResilienceOverrides
{
    /// <summary>
    /// Gets the specific timeout for the request.
    /// </summary>
    TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets the specific retry count for the request.
    /// </summary>
    int? RetryCount { get; init; }
}
