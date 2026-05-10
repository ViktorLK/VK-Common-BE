using System;

namespace VK.Blocks.Resilience;

/// <summary>
/// Configuration options for retry strategies.
/// </summary>
public sealed record VKRetryOptions
{
    /// <summary>
    /// Gets the number of retry attempts.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Gets the backoff delay between retries.
    /// </summary>
    public TimeSpan Backoff { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets whether to use exponential backoff.
    /// </summary>
    public bool UseExponentialBackoff { get; init; } = true;
}
