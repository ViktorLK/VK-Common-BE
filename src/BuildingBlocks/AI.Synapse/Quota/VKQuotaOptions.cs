using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Configuration options for AI Synapse Quota & Rate Limiting feature slice.
/// </summary>
public sealed partial record VKQuotaOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the default cooldown duration when circuit breaker opens.
    /// </summary>
    public TimeSpan DefaultCooldownDuration { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets the default failure threshold count before circuit breaker opens.
    /// </summary>
    public int DefaultCircuitBreakerThreshold { get; init; } = 3;

    /// <summary>
    /// Gets the default maximum concurrency per connection.
    /// </summary>
    public int DefaultMaxConcurrency { get; init; } = 10;

    /// <summary>
    /// Gets the default requests per minute (RPM) limit per connection. Default is 60 RPM (0 disables RPM limit).
    /// </summary>
    public int DefaultRequestsPerMinute { get; init; } = 60;

    /// <summary>
    /// Gets the default tokens per minute limit. Default is 100,000 TPM.
    /// </summary>
    public int DefaultTokensPerMinute { get; init; } = 100_000;

    /// <summary>
    /// Gets whether token rate limiting (TPM) and budgeting are enabled.
    /// </summary>
    public bool EnableTokenBudget { get; init; } = true;
}
