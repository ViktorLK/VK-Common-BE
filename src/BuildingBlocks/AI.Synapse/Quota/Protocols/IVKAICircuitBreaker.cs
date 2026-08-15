using System;
using System.Collections.Generic;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Manages the health and cooldown states of AI connections.
/// </summary>
public interface IVKAICircuitBreaker
{
    /// <summary>
    /// Checks if the connection is allowed to execute requests (i.e. not in cooldown).
    /// </summary>
    bool IsAllowed(VKAIConnection connection);

    /// <summary>
    /// Records a successful request execution, resetting failure metrics.
    /// </summary>
    void RecordSuccess(VKAIConnection connection);

    /// <summary>
    /// Records a failed request execution, tripping the circuit breaker if threshold is met.
    /// </summary>
    void RecordFailure(VKAIConnection connection, Exception ex);

    /// <summary>
    /// Gets all connections that are currently in cooldown.
    /// </summary>
    IReadOnlyList<VKAIConnection> GetProvidersOnCooldown();
}
