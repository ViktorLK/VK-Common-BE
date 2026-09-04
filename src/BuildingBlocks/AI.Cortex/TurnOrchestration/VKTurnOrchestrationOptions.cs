using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Options for Turn Orchestration slice.
/// Follows BB.05.
/// </summary>
public sealed partial record VKTurnOrchestrationOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether Turn Orchestration is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the optional custom timeout for dialogue turns.
    /// If null, defaults to <see cref="CortexConstants.Resilience.DefaultChatTimeout"/> (30s).
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Gets the optional custom retry count for dialogue turns.
    /// If null, defaults to <see cref="CortexConstants.Resilience.DefaultChatMaxRetries"/> (3).
    /// </summary>
    public int? RetryCount { get; init; }

    /// <summary>
    /// Gets the optional circuit breaker key for dialogue turns.
    /// </summary>
    public string? CircuitBreakerKey { get; init; }
}
