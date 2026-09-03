using System;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Configuration options for the circuit breaker feature slice.
/// Follows [AP.01], [BB.05].
/// </summary>
public sealed partial record VKCircuitBreakerOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the percentage of failures (0.0 to 1.0) allowed before opening the circuit.
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

    /// <summary>
    /// Gets the number of trial calls permitted when the circuit breaker is in half-open state.
    /// </summary>
    public int PermittedNumberOfCallsInHalfOpenState { get; init; } = 1;

    /// <summary>
    /// Gets the sampling duration window for tracking failures.
    /// </summary>
    public TimeSpan SamplingDuration { get; init; } = TimeSpan.FromSeconds(30);
}
