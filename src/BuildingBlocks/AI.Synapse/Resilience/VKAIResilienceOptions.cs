using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Configuration options for AI resilience, provider failover, and rate-limit retry policies.
/// Follows [AP.01], [BB.05].
/// </summary>
public sealed partial record VKAIResilienceOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets the cooldown duration before attempting to retry an open circuit for a provider.
    /// </summary>
    public TimeSpan ProviderCooldown { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the default base delay for rate-limit retries if no Retry-After header is present.
    /// </summary>
    public TimeSpan DefaultRateLimitRetryDelay { get; init; } = TimeSpan.FromSeconds(2);
}
