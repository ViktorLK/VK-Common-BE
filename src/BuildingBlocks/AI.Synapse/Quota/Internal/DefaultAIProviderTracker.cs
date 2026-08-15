using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Internal;

// [AP.01] sealed
internal sealed class DefaultAIProviderTracker : IVKAIProviderTracker
{
    private readonly IVKAICircuitBreaker _circuitBreaker;
    private readonly IVKAIRateLimiter _rateLimiter;
    private readonly IVKAIMetricsCollector _metricsCollector;

    public DefaultAIProviderTracker(
        IVKAICircuitBreaker circuitBreaker,
        IVKAIRateLimiter rateLimiter,
        IVKAIMetricsCollector metricsCollector)
    {
        _circuitBreaker = VKGuard.NotNull(circuitBreaker);
        _rateLimiter = VKGuard.NotNull(rateLimiter);
        _metricsCollector = VKGuard.NotNull(metricsCollector);
    }

    public bool IsAvailable(VKAIConnection connection)
    {
        if (connection is null)
            return false;
        return _circuitBreaker.IsAllowed(connection) && _rateLimiter.IsAllowed(connection);
    }

    public void RecordRequest(VKAIConnection connection)
    {
        if (connection is null)
            return;
        _rateLimiter.Acquire(connection);
    }

    public void MarkSuccess(VKAIConnection connection)
    {
        if (connection is null)
            return;
        _rateLimiter.Release(connection);
        _circuitBreaker.RecordSuccess(connection);
    }

    public void MarkFailure(VKAIConnection connection, Exception ex)
    {
        if (connection is null)
            return;
        _rateLimiter.Release(connection);
        _circuitBreaker.RecordFailure(connection, ex);
    }

    public void RecordMetrics(VKAIConnection connection, int tokens, TimeSpan latency)
    {
        if (connection is null)
            return;
        _metricsCollector.RecordMetrics(connection, tokens, latency);
    }

    public IReadOnlyList<VKAIConnection> GetProvidersOnCooldown()
    {
        return _circuitBreaker.GetProvidersOnCooldown();
    }
}
