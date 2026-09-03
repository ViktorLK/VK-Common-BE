using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Common.Internal.Policies;

namespace VK.Blocks.Resilience.Common.Internal;

// [AP.01] sealed
internal sealed class DefaultPolicyBuilder : IVKPolicyBuilder
{
    private readonly List<IVKResiliencePolicy> _policies = new();
    private readonly IServiceProvider _serviceProvider;

    public string PipelineName { get; }

    public DefaultPolicyBuilder(string pipelineName, IServiceProvider serviceProvider)
    {
        PipelineName = VKGuard.NotNullOrWhiteSpace(pipelineName);
        _serviceProvider = VKGuard.NotNull(serviceProvider);
    }

    public IVKPolicyBuilder AddPolicy(IVKResiliencePolicy policy)
    {
        VKGuard.NotNull(policy);
        _policies.Add(policy);
        return this;
    }

    public IVKPolicyBuilder AddTimeout(
        TimeSpan timeout,
        bool isPessimistic = false,
        Action<TimeSpan, VKResilienceContext>? onTimeout = null)
    {
        var executor = _serviceProvider.GetRequiredService<IVKTimeoutExecutor>();
        _policies.Add(new TimeoutResiliencePolicy(timeout, isPessimistic, onTimeout, executor));
        return this;
    }

    public IVKPolicyBuilder AddRetry(
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        double backoffMultiplier = 2.0,
        bool useJitter = true,
        Func<VKError, bool>? shouldRetry = null,
        Action<int, TimeSpan, VKError, VKResilienceContext>? onRetry = null)
    {
        var executor = _serviceProvider.GetRequiredService<IVKRetryExecutor>();
        _policies.Add(new RetryResiliencePolicy(
            maxRetries,
            initialDelay,
            backoffMultiplier,
            useJitter,
            shouldRetry,
            onRetry,
            executor));
        return this;
    }

    public IVKPolicyBuilder AddCircuitBreaker(
        string circuitBreakerKey,
        TimeSpan? durationOfBreak = null,
        int minimumThroughput = 10,
        double failureRatio = 0.5,
        int halfOpenPermittedCalls = 1,
        Action<string, TimeSpan, VKResilienceContext>? onBreak = null,
        Action<string, VKResilienceContext>? onReset = null)
    {
        var circuitBreaker = _serviceProvider.GetRequiredService<IVKCircuitBreaker>();
        _policies.Add(new CircuitBreakerResiliencePolicy(
            circuitBreakerKey,
            durationOfBreak,
            minimumThroughput,
            failureRatio,
            halfOpenPermittedCalls,
            onBreak,
            onReset,
            circuitBreaker));
        return this;
    }

    public IVKPolicyBuilder AddRateLimiter(
        string rateLimiterKey,
        int permitLimit,
        TimeSpan? window = null)
    {
        var limiter = _serviceProvider.GetRequiredService<IVKRateLimiter>();
        _policies.Add(new RateLimiterResiliencePolicy(rateLimiterKey, permitLimit, window, limiter));
        return this;
    }

    public IVKPolicyBuilder AddTokenBucket(
        string tokenBucketKey,
        double tokensPerSecond,
        double maxBurstTokens)
    {
        var limiter = _serviceProvider.GetRequiredService<IVKTokenBucketLimiter>();
        limiter.ConfigureBucket(tokenBucketKey, tokensPerSecond, maxBurstTokens);
        _policies.Add(new TokenBucketResiliencePolicy(tokenBucketKey, 1.0, null, limiter));
        return this;
    }

    public IVKPolicyBuilder AddBulkhead(
        string bulkheadKey,
        int maxParallelization,
        int maxQueuedCount = 0,
        TimeSpan? queueTimeout = null)
    {
        var bulkhead = _serviceProvider.GetRequiredService<IVKBulkhead>();
        _policies.Add(new BulkheadResiliencePolicy(
            bulkheadKey,
            maxParallelization,
            maxQueuedCount,
            queueTimeout,
            bulkhead));
        return this;
    }

    public IVKResiliencePipeline Build()
    {
        return new DefaultResiliencePipeline(PipelineName, _policies);
    }
}
