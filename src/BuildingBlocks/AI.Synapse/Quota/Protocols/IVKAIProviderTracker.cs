using System;
using System.Collections.Generic;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Tracks the health, cooldowns, and rate limits of AI connections dynamically.
/// </summary>
public interface IVKAIProviderTracker
{
    /// <summary>
    /// Checks if the connection is currently available (not on cooldown and within rate limits).
    /// </summary>
    bool IsAvailable(VKAIConnection connection);

    /// <summary>
    /// Marks that a request to the connection failed.
    /// </summary>
    void MarkFailure(VKAIConnection connection, Exception ex);

    /// <summary>
    /// Marks that a request to the connection succeeded, resolving cooldown.
    /// </summary>
    void MarkSuccess(VKAIConnection connection);

    /// <summary>
    /// Records a successful request for rate limit tracking.
    /// </summary>
    void RecordRequest(VKAIConnection connection);

    /// <summary>
    /// Records the latency and token usage metrics for a completed request.
    /// </summary>
    void RecordMetrics(VKAIConnection connection, int tokens, TimeSpan latency);

    /// <summary>
    /// Gets all connections that are currently in cooldown.
    /// </summary>
    IReadOnlyList<VKAIConnection> GetProvidersOnCooldown();
}
