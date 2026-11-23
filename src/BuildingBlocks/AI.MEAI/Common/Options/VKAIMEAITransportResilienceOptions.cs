using System;

namespace VK.Blocks.AI.MEAI;

/// <summary>
/// Configuration options governing outbound transport resilience for AI provider calls.
/// Adheres to BB.05, BB.07, and MEA-06.
/// </summary>
// [AP.01] [BB.05] [BB.07]
public sealed record VKAIMEAITransportResilienceOptions
{
    /// <summary>
    /// Gets the resilience pipeline name registered in <see cref="Resilience.IVKPolicyRegistry"/>.
    /// Defaults to <see cref="VKAIConstants.AIModelCallPipelineName"/>.
    /// </summary>
    public string PipelineName { get; init; } = VKAIConstants.AIModelCallPipelineName;

    /// <summary>
    /// Gets the maximum number of transport-level retry attempts per connection attempt.
    /// Defaults to 2.
    /// </summary>
    public int MaxRetries { get; init; } = 2;

    /// <summary>
    /// Gets the per-attempt timeout applied to each individual provider HTTP attempt.
    /// Defaults to 8 seconds so <c>(1 + MaxRetries) * AttemptTimeout &lt;= 30s</c> turn deadline.
    /// </summary>
    public TimeSpan AttemptTimeout { get; init; } = TimeSpan.FromSeconds(8);

    /// <summary>
    /// Gets the initial delay before the first transport retry.
    /// Defaults to 500 milliseconds.
    /// </summary>
    public TimeSpan InitialRetryDelay { get; init; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets the exponential backoff multiplier between transport retries.
    /// Defaults to 2.0.
    /// </summary>
    public double BackoffMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Gets the cross-layer cap on total provider HTTP attempts across failover legs and transport retries.
    /// Defaults to 4.
    /// </summary>
    public int MaxTotalAttempts { get; init; } = 4;

    /// <summary>
    /// Gets a value indicating whether a no-op resilience configuration is permitted at startup.
    /// Defaults to <see langword="false"/> per [AP.06].
    /// </summary>
    public bool AllowNoOpResilience { get; init; } = false;
}
