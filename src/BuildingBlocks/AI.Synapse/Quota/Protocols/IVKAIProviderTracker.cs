using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Tracks the health, cooldowns, and rate limits of AI connections dynamically.
/// </summary>
public interface IVKAIProviderTracker
{
    /// <summary>
    /// Checks non-destructively whether the connection is currently available (not open/isolated and within rate/concurrency limits).
    /// </summary>
    bool IsAvailable(VKAIConnection connection);

    /// <summary>
    /// Records the latency and token usage metrics for a completed request.
    /// </summary>
    void RecordMetrics(VKAIConnection connection, int tokens, TimeSpan latency);

    /// <summary>
    /// Gets all connections that are currently open/in cooldown or isolated.
    /// </summary>
    IReadOnlyList<VKAIConnection> GetProvidersOnCooldown();

    /// <summary>
    /// Asynchronously gets all connections that are currently open/in cooldown or isolated.
    /// </summary>
    Task<IReadOnlyList<VKAIConnection>> GetProvidersOnCooldownAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(GetProvidersOnCooldown());
}
