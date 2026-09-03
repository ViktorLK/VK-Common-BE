using System;
using System.Collections.Generic;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for tracking and managing circuit breaker state.
/// Supports Closed, Open, and HalfOpen states with trial execution limits.
/// Follows [AP.01], [CS.01].
/// </summary>
public interface IVKCircuitBreaker
{
    /// <summary>
    /// Gets the current state of the circuit breaker for the specified key.
    /// </summary>
    VKCircuitState GetState(string key);

    /// <summary>
    /// Checks if operations for the specified key are allowed (Closed or permitted HalfOpen call).
    /// </summary>
    bool IsAllowed(string key);

    /// <summary>
    /// Records a successful execution, resetting failure metrics or transitioning from HalfOpen to Closed.
    /// </summary>
    void RecordSuccess(string key, int? failureThreshold = null, Action<string>? onReset = null);

    /// <summary>
    /// Records a failed execution, tripping the circuit breaker if threshold is met or immediately opening if in HalfOpen.
    /// </summary>
    void RecordFailure(
        string key,
        Exception ex,
        TimeSpan? cooldownDuration = null,
        int? failureThreshold = null,
        double? failureRatio = null,
        Action<string, TimeSpan>? onBreak = null);

    /// <summary>
    /// Gets all keys that are currently in Open (cooldown) state.
    /// </summary>
    IReadOnlyList<string> GetKeysOnCooldown();
}
