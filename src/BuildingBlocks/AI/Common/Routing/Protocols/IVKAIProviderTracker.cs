using System;
using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Tracks the health, cooldowns, and rate limits of AI providers dynamically.
/// </summary>
public interface IVKAIProviderTracker
{
    /// <summary>
    /// Checks if the provider is currently available (not on cooldown and within rate limits).
    /// </summary>
    bool IsAvailable(IVKAIProviderOptions config);

    /// <summary>
    /// Marks that a request to the provider failed.
    /// </summary>
    void MarkFailure(IVKAIProviderOptions config, Exception ex);

    /// <summary>
    /// Marks that a request to the provider succeeded, resolving cooldown.
    /// </summary>
    void MarkSuccess(IVKAIProviderOptions config);

    /// <summary>
    /// Records a successful request for rate limit tracking.
    /// </summary>
    void RecordRequest(IVKAIProviderOptions config);

    /// <summary>
    /// Records the latency and token usage metrics for a completed request.
    /// </summary>
    void RecordMetrics(IVKAIProviderOptions config, int tokens, TimeSpan latency);

    /// <summary>
    /// Gets all providers that are currently in cooldown.
    /// </summary>
    IReadOnlyList<IVKAIProviderOptions> GetProvidersOnCooldown();
}
