using System;

namespace VK.Blocks.AI;

/// <summary>
/// Provides a base implementation for AI resilience settings.
/// </summary>
public sealed record VKAIResilienceSettings : IVKAIResilienceOptions
{
    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <inheritdoc />
    public int? RetryCount { get; init; }

    /// <inheritdoc />
    public int? CircuitBreakerThreshold { get; init; }

    /// <inheritdoc />
    public TimeSpan? CircuitBreakerBreakDuration { get; init; }
}
