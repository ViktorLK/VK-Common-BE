using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Defines the contract for AI/LLM resilience operations (Provider failover, Model downgrade, and dynamic RateLimit retry).
/// Follows [AP.01], [CS.01], [CS.03].
/// </summary>
public interface IVKAIResilienceProvider
{
    /// <summary>
    /// Executes an AI call with automatic provider fallback (e.g. OpenAI -> Gemini failover).
    /// </summary>
    Task<VKResult<T>> ExecuteWithProviderFallbackAsync<T>(
        string primaryProviderName,
        string fallbackProviderName,
        Func<string, CancellationToken, Task<VKResult<T>>> executeWithProvider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an AI call with automatic model downgrade fallback (e.g. GPT-4 -> GPT-4o-mini).
    /// </summary>
    Task<VKResult<T>> ExecuteWithModelFallbackAsync<T>(
        string primaryModelId,
        string fallbackModelId,
        Func<string, CancellationToken, Task<VKResult<T>>> executeWithModel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an AI operation with intelligent RateLimit retry respecting Retry-After header hints.
    /// </summary>
    Task<VKResult<T>> ExecuteWithRateLimitRetryAsync<T>(
        Func<CancellationToken, Task<VKResult<T>>> action,
        int maxRetries = 3,
        TimeSpan? defaultRetryAfter = null,
        CancellationToken cancellationToken = default);
}
