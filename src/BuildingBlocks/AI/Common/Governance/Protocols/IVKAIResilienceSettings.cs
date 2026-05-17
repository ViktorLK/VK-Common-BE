using System;

namespace VK.Blocks.AI;

/// <summary>
/// Defines resilience parameters for AI features.
/// </summary>
public interface IVKAIResilienceSettings
{
    /// <summary>
    /// Gets the specific timeout for the feature.
    /// </summary>
    TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets the specific retry count for the feature.
    /// </summary>
    int? RetryCount { get; init; }

    /// <summary>
    /// Gets the specific circuit breaker threshold for the feature.
    /// </summary>
    int? CircuitBreakerThreshold { get; init; }

    /// <summary>
    /// Gets the specific circuit breaker break duration for the feature.
    /// </summary>
    TimeSpan? CircuitBreakerBreakDuration { get; init; }
}
