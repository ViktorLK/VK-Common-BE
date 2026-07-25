using System;
using System.Collections.Generic;

namespace VK.Blocks.AI.Gateway.Internal;

internal sealed class AIProviderTracker : IVKAIProviderTracker
{
    private readonly IAICircuitBreaker _circuitBreaker;
    private readonly IAIRateLimiter _rateLimiter;
    private readonly IAIMetricsCollector _metricsCollector;

    public AIProviderTracker(
        IAICircuitBreaker circuitBreaker,
        IAIRateLimiter rateLimiter,
        IAIMetricsCollector metricsCollector)
    {
        _circuitBreaker = circuitBreaker;
        _rateLimiter = rateLimiter;
        _metricsCollector = metricsCollector;
    }

    public bool IsAvailable(IVKAIProviderOptions config)
    {
        if (config == null)
            return false;
        return _circuitBreaker.IsAllowed(config) && _rateLimiter.IsAllowed(config);
    }

    public void MarkFailure(IVKAIProviderOptions config, Exception ex)
    {
        if (config == null)
            return;
        _rateLimiter.Release(config);
        _circuitBreaker.RecordFailure(config, ex);
    }

    public void MarkSuccess(IVKAIProviderOptions config)
    {
        if (config == null)
            return;
        _rateLimiter.Release(config);
        _circuitBreaker.RecordSuccess(config);
    }

    public void RecordRequest(IVKAIProviderOptions config)
    {
        if (config == null)
            return;
        _rateLimiter.Acquire(config);
    }

    public void RecordMetrics(IVKAIProviderOptions config, int tokens, TimeSpan latency)
    {
        if (config == null)
            return;
        _rateLimiter.Release(config);
        _metricsCollector.RecordMetrics(config, tokens, latency);
        _circuitBreaker.RecordSuccess(config);
    }

    public IReadOnlyList<IVKAIProviderOptions> GetProvidersOnCooldown()
    {
        return _circuitBreaker.GetProvidersOnCooldown();
    }
}
