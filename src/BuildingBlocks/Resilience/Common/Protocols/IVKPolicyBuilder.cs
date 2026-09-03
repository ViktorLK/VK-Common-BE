using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience;

/// <summary>
/// Defines the fluent builder for configuring and creating <see cref="IVKResiliencePipeline"/> instances.
/// Follows [AP.01], [BB.05], [CS.01].
/// </summary>
public interface IVKPolicyBuilder
{
    /// <summary>
    /// Gets the name of the pipeline being built.
    /// </summary>
    string PipelineName { get; }

    /// <summary>
    /// Adds a custom resilience policy to the pipeline.
    /// </summary>
    IVKPolicyBuilder AddPolicy(IVKResiliencePolicy policy);

    /// <summary>
    /// Adds a timeout policy to the pipeline.
    /// </summary>
    IVKPolicyBuilder AddTimeout(
        TimeSpan timeout,
        bool isPessimistic = false,
        Action<TimeSpan, VKResilienceContext>? onTimeout = null);

    /// <summary>
    /// Adds a retry policy with exponential backoff and jitter to the pipeline.
    /// </summary>
    IVKPolicyBuilder AddRetry(
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        double backoffMultiplier = 2.0,
        bool useJitter = true,
        Func<VKError, bool>? shouldRetry = null,
        Action<int, TimeSpan, VKError, VKResilienceContext>? onRetry = null);

    /// <summary>
    /// Adds a circuit breaker policy to the pipeline for the specified key.
    /// </summary>
    IVKPolicyBuilder AddCircuitBreaker(
        string circuitBreakerKey,
        TimeSpan? durationOfBreak = null,
        int minimumThroughput = 10,
        double failureRatio = 0.5,
        int halfOpenPermittedCalls = 1,
        Action<string, TimeSpan, VKResilienceContext>? onBreak = null,
        Action<string, VKResilienceContext>? onReset = null);

    /// <summary>
    /// Adds a sliding-window rate limiter policy to the pipeline.
    /// </summary>
    IVKPolicyBuilder AddRateLimiter(
        string rateLimiterKey,
        int permitLimit,
        TimeSpan? window = null);

    /// <summary>
    /// Adds a token-bucket rate limiter policy to the pipeline.
    /// </summary>
    IVKPolicyBuilder AddTokenBucket(
        string tokenBucketKey,
        double tokensPerSecond,
        double maxBurstTokens);

    /// <summary>
    /// Adds a bulkhead concurrency and queuing policy to the pipeline.
    /// </summary>
    IVKPolicyBuilder AddBulkhead(
        string bulkheadKey,
        int maxParallelization,
        int maxQueuedCount = 0,
        TimeSpan? queueTimeout = null);

    /// <summary>
    /// Builds the configured <see cref="IVKResiliencePipeline"/>.
    /// </summary>
    IVKResiliencePipeline Build();
}
