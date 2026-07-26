using System;
using System.Collections.Generic;

namespace VK.Blocks.AI.Gateway.Internal;

/// <summary>
/// Manages the health and cooldown states of AI providers.
/// </summary>
internal interface IAICircuitBreaker
{
    /// <summary>
    /// Checks if the provider is allowed to execute requests (i.e. not in cooldown).
    /// </summary>
    bool IsAllowed(IVKAIProviderOptions config);

    /// <summary>
    /// Records a successful request execution, resetting failure metrics.
    /// </summary>
    void RecordSuccess(IVKAIProviderOptions config);

    /// <summary>
    /// Records a failed request execution, tripping the circuit breaker if threshold is met.
    /// </summary>
    void RecordFailure(IVKAIProviderOptions config, Exception ex);

    /// <summary>
    /// Gets all providers that are currently in cooldown.
    /// </summary>
    IReadOnlyList<IVKAIProviderOptions> GetProvidersOnCooldown();
}
