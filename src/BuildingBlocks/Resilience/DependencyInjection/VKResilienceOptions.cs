using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Root configuration options for the Resilience building block.
/// </summary>
public sealed record VKResilienceOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the section name for the configuration.
    /// </summary>
    public static string SectionName => "Resilience";

    /// <summary>
    /// Gets or sets a value indicating whether the block is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the default retry options.
    /// </summary>
    public VKRetryOptions DefaultRetry { get; init; } = new();

    /// <summary>
    /// Gets the default circuit breaker options.
    /// </summary>
    public VKCircuitBreakerOptions DefaultCircuitBreaker { get; init; } = new();

    /// <summary>
    /// Gets the default timeout options.
    /// </summary>
    public VKTimeoutOptions DefaultTimeout { get; init; } = new();

    /// <summary>
    /// Gets the default bulkhead options.
    /// </summary>
    public VKBulkheadOptions DefaultBulkhead { get; init; } = new();
}
