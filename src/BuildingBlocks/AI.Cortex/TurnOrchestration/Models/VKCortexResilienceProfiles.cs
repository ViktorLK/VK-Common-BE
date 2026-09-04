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
    public static VKStepResiliencePolicy ChatCompletionProfile { get; } = CreateChatProfile();

    /// <summary>
    /// Creates a scenario-based resilience profile for standard LLM Chat completions with custom timeout and retries.
    /// </summary>
    public static VKStepResiliencePolicy CreateChatProfile(
        TimeSpan? timeout = null,
        int? maxRetries = null,
        string? circuitBreakerKey = null)
    {
        return VKStepResiliencePolicy.Default
            .WithTimeout(timeout ?? CortexConstants.Resilience.DefaultChatTimeout)
            .WithCircuitBreaker(circuitBreakerKey ?? CortexConstants.Resilience.DefaultLlmCircuitBreakerKey)
            .WithRetry(VKStepRetryPolicy.Default with
            {
                MaxRetries = maxRetries ?? CortexConstants.Resilience.DefaultChatMaxRetries,
                InitialDelayMs = CortexConstants.Resilience.DefaultChatInitialDelayMs,
                BackoffMultiplier = CortexConstants.Resilience.DefaultChatBackoffMultiplier,
                UseJitter = true
            });
    }

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
