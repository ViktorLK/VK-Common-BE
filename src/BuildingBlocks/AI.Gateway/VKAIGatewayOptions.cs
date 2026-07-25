using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Gateway;

/// <summary>
/// Default configuration properties for the AI Gateway block.
/// </summary>
public sealed partial record VKAIGatewayOptions : IVKBlockOptions
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
}
