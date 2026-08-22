using System;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Preset scenario-based resilience profiles for AI operations.
/// Follows [AP.01] and [AP.06].
/// </summary>
public static class VKCortexResilienceProfiles
{
    /// <summary>
    /// Default profile for standard LLM Chat completions:
    /// 3x exponential backoff with jitter on transient errors (429/503/timeout),
    /// 30-second single-call timeout, and circuit breaker protection.
    /// </summary>
    public static VKStepResiliencePolicy ChatCompletionProfile { get; } = VKStepResiliencePolicy.Default
        .WithTimeout(CortexConstants.Resilience.DefaultChatTimeout)
        .WithCircuitBreaker(CortexConstants.Resilience.DefaultLlmCircuitBreakerKey)
        .WithRetry(VKStepRetryPolicy.Default with
        {
            MaxRetries = CortexConstants.Resilience.DefaultChatMaxRetries,
            InitialDelayMs = CortexConstants.Resilience.DefaultChatInitialDelayMs,
            BackoffMultiplier = CortexConstants.Resilience.DefaultChatBackoffMultiplier,
            UseJitter = true
        });

    /// <summary>
    /// Profile for fast tool calls or small embeddings:
    /// 10-second timeout with 1 fast retry.
    /// </summary>
    public static VKStepResiliencePolicy FastToolCallProfile { get; } = VKStepResiliencePolicy.Default
        .WithTimeout(CortexConstants.Resilience.DefaultFastToolTimeout)
        .WithCircuitBreaker(CortexConstants.Resilience.DefaultFastToolCircuitBreakerKey)
        .WithRetry(VKStepRetryPolicy.Default with
        {
            MaxRetries = CortexConstants.Resilience.DefaultFastToolMaxRetries,
            InitialDelayMs = CortexConstants.Resilience.DefaultFastToolInitialDelayMs,
            UseJitter = false
        });
}
