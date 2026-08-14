using System;
using System.Collections.Generic;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the contract for tracking and managing circuit breaker state.
/// </summary>
public interface IVKCircuitBreaker
{
    /// <summary>
    /// Checks if operations for the specified key are allowed (i.e. not in cooldown).
    /// </summary>
    bool IsAllowed(string key);

    /// <summary>
    /// Records a successful execution, resetting failure metrics.
    /// </summary>
    void RecordSuccess(string key, int? failureThreshold = null);

    /// <summary>
    /// Records a failed execution, tripping the circuit breaker if threshold is met.
    /// </summary>
    void RecordFailure(string key, Exception ex, TimeSpan? cooldownDuration = null, int? failureThreshold = null, double? failureRatio = null);

    /// <summary>
    /// Gets all keys that are currently in cooldown.
    /// </summary>
    IReadOnlyList<string> GetKeysOnCooldown();
}
