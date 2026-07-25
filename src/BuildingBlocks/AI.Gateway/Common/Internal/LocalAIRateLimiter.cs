using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Gateway.Internal;

internal sealed class LocalAIRateLimiter : IAIRateLimiter
{
    private sealed class LimiterState
    {
        public List<DateTimeOffset> RequestTimestamps { get; } = new();
        public int InFlightRequests { get; set; }
        public object LockObject { get; } = new();
    }

    private readonly ConcurrentDictionary<string, LimiterState> _states = new();
    private readonly TimeProvider _timeProvider;
    private readonly VKAIGatewayOptions _defaults;

    public LocalAIRateLimiter(IOptions<VKAIGatewayOptions> defaults, TimeProvider? timeProvider = null)
    {
        _defaults = defaults.Value;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public bool IsAllowed(IVKAIProviderOptions config)
    {
        if (config == null)
            return false;
        var state = GetOrCreateState(config);
        var now = _timeProvider.GetUtcNow();

        lock (state.LockObject)
        {
            if (state.InFlightRequests >= _defaults.DefaultMaxConcurrency)
            {
                return false;
            }

            CleanOldRequests(state, now);
            if (config is IVKAIQuotaOptions quota && quota.RateLimitPerMinute.HasValue)
            {
                int limit = quota.RateLimitPerMinute.Value;
                if (limit > 0 && state.RequestTimestamps.Count >= limit)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void Acquire(IVKAIProviderOptions config)
    {
        if (config == null)
            return;
        var state = GetOrCreateState(config);
        var now = _timeProvider.GetUtcNow();

        lock (state.LockObject)
        {
            state.InFlightRequests++;
            state.RequestTimestamps.Add(now);
        }
    }

    public void Release(IVKAIProviderOptions config)
    {
        if (config == null)
            return;
        var state = GetOrCreateState(config);

        lock (state.LockObject)
        {
            state.InFlightRequests = Math.Max(0, state.InFlightRequests - 1);
        }
    }

    private LimiterState GetOrCreateState(IVKAIProviderOptions config)
    {
        var key = GetProviderKey(config);
        return _states.GetOrAdd(key, _ => new LimiterState());
    }

    private string GetProviderKey(IVKAIProviderOptions options)
    {
        var keyStr = options.ApiKey?.ToString() ?? string.Empty;
        return $"{options.Provider}_{options.Endpoint}_{options.ModelId}_{keyStr.GetHashCode()}";
    }

    private void CleanOldRequests(LimiterState state, DateTimeOffset now)
    {
        var oneMinuteAgo = now.AddMinutes(-1);
        state.RequestTimestamps.RemoveAll(t => t < oneMinuteAgo);
    }
}
