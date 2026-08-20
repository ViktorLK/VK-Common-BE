using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Step-level circuit breaker policy referencing the shared singleton circuit breaker state for a specific key.
/// Follows AP.01, BB.05, and AP.07 (holds key reference only; cooldown and threshold are configured globally).
/// </summary>
public sealed record VKStepCircuitBreakerPolicy
{
    /// <summary>
    /// Gets the unique circuit breaker key (e.g. LLM provider or external downstream service name).
    /// </summary>
    public required string CircuitBreakerKey { get; init; }

    /// <summary>
    /// Creates a circuit breaker policy for the specified key.
    /// </summary>
    public static VKStepCircuitBreakerPolicy ForKey(string circuitBreakerKey)
    {
        VKGuard.NotNullOrWhiteSpace(circuitBreakerKey);
        return new VKStepCircuitBreakerPolicy { CircuitBreakerKey = circuitBreakerKey };
    }
}
