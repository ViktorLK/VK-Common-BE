using System;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Configuration options for the circuit breaker feature slice.
/// </summary>
public sealed partial record VKCircuitBreakerOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the percentage of failures allowed before opening the circuit.
    /// </summary>
    public double FailureThreshold { get; init; } = 0.5;

    /// <summary>
    /// Gets the duration the circuit remains open before transitioning to half-open.
    /// </summary>
    public TimeSpan DurationOfBreak { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets the minimum throughput required before failure threshold is evaluated.
    /// </summary>
    public int MinimumThroughput { get; init; } = 20;
}
