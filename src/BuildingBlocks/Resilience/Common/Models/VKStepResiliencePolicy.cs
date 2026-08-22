using System;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Composite step-level resilience policy combining independent slice configurations:
/// Retry (<see cref="VKStepRetryPolicy"/>), Timeout (<see cref="VKStepTimeoutPolicy"/>), and CircuitBreaker (<see cref="VKStepCircuitBreakerPolicy"/>).
/// Follows [AP.01], [BB.05], [BB.07], and provides seamless backward compatibility with <see cref="VKStepRetryPolicy"/>.
/// </summary>
public sealed record VKStepResiliencePolicy
{
    /// <summary>
    /// Gets the retry policy configuration for this step, or null if retry is disabled.
    /// </summary>
    public VKStepRetryPolicy? Retry { get; init; } = VKStepRetryPolicy.Default;

    /// <summary>
    /// Gets the timeout policy configuration for this step, or null if timeout is disabled.
    /// </summary>
    public VKStepTimeoutPolicy? Timeout { get; init; }

    /// <summary>
    /// Gets the circuit breaker policy configuration for this step, or null if circuit breaker is not used.
    /// </summary>
    public VKStepCircuitBreakerPolicy? CircuitBreaker { get; init; }

    /// <summary>
    /// Default resilience policy (standard 3x retry, default timeout / circuit breaker disabled).
    /// </summary>
    public static VKStepResiliencePolicy Default { get; } = new();

    /// <summary>
    /// No resilience policy: Fail immediately on any error without retries or timeouts.
    /// </summary>
    public static VKStepResiliencePolicy None { get; } = new() { Retry = VKStepRetryPolicy.None };

    /// <summary>
    /// Implicit conversion from <see cref="VKStepRetryPolicy"/> for backward compatibility.
    /// </summary>
    public static implicit operator VKStepResiliencePolicy(VKStepRetryPolicy retryPolicy)
        => new() { Retry = retryPolicy };

    /// <summary>
    /// Fluent helper to attach or replace the retry policy.
    /// </summary>
    public VKStepResiliencePolicy WithRetry(VKStepRetryPolicy? retryPolicy)
        => this with { Retry = retryPolicy };

    /// <summary>
    /// Fluent helper to attach or replace the timeout policy.
    /// </summary>
    public VKStepResiliencePolicy WithTimeout(VKStepTimeoutPolicy? timeoutPolicy)
        => this with { Timeout = timeoutPolicy };

    /// <summary>
    /// Fluent helper to attach or replace the timeout policy from a <see cref="TimeSpan"/>.
    /// </summary>
    public VKStepResiliencePolicy WithTimeout(TimeSpan timeout)
        => this with { Timeout = VKStepTimeoutPolicy.FromTimeSpan(timeout) };

    /// <summary>
    /// Fluent helper to attach or replace the circuit breaker policy.
    /// </summary>
    public VKStepResiliencePolicy WithCircuitBreaker(VKStepCircuitBreakerPolicy? circuitBreakerPolicy)
        => this with { CircuitBreaker = circuitBreakerPolicy };

    /// <summary>
    /// Fluent helper to attach or replace the circuit breaker policy with the specified key.
    /// </summary>
    public VKStepResiliencePolicy WithCircuitBreaker(string circuitBreakerKey)
    {
        VKGuard.NotNullOrWhiteSpace(circuitBreakerKey);
        return this with { CircuitBreaker = VKStepCircuitBreakerPolicy.ForKey(circuitBreakerKey) };
    }
}
