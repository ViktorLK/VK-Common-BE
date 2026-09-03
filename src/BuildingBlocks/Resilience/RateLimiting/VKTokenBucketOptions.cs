using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Configuration options for the token bucket rate limiter.
/// Follows [AP.01], [BB.05].
/// </summary>
public sealed partial record VKTokenBucketOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the default token replenishment rate per second.
    /// </summary>
    public double DefaultTokensPerSecond { get; init; } = 10.0;

    /// <summary>
    /// Gets the default maximum capacity / burst size of the token bucket.
    /// </summary>
    public double DefaultMaxBurstTokens { get; init; } = 20.0;
}
