using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace VK.Blocks.AI.Gateway.Internal;

internal sealed class LocalAICircuitBreaker : IAICircuitBreaker
{
    private sealed class BreakerState
    {
        public IVKAIProviderOptions Config { get; }
        public DateTimeOffset? CooldownUntil { get; set; }
        public Queue<bool> RecentSuccesses { get; } = new();
        public object LockObject { get; } = new();

        public BreakerState(IVKAIProviderOptions config)
        {
            Config = config;
        }
    }

    private readonly ConcurrentDictionary<string, BreakerState> _states = new();
    private readonly TimeProvider _timeProvider;
    private readonly VKAIGatewayOptions _defaults;

    public LocalAICircuitBreaker(IOptions<VKAIGatewayOptions> defaults, TimeProvider? timeProvider = null)
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
            if (state.CooldownUntil.HasValue && state.CooldownUntil.Value > now)
            {
                return false;
            }
        }
        return true;
    }

    public void RecordSuccess(IVKAIProviderOptions config)
    {
        if (config == null)
            return;
        var state = GetOrCreateState(config);

        lock (state.LockObject)
        {
            state.RecentSuccesses.Enqueue(true);
            var failureThreshold = _defaults.DefaultCircuitBreakerThreshold;
            if (config is IVKAIResilienceOptions resilience && resilience.CircuitBreakerThreshold.HasValue)
            {
                failureThreshold = resilience.CircuitBreakerThreshold.Value;
            }
            int maxHistory = Math.Max(5, failureThreshold * 2);
            if (state.RecentSuccesses.Count > maxHistory)
            {
                state.RecentSuccesses.Dequeue();
            }
            state.CooldownUntil = null;
        }
    }

    public void RecordFailure(IVKAIProviderOptions config, Exception ex)
    {
        if (config == null)
            return;
        var state = GetOrCreateState(config);
        var now = _timeProvider.GetUtcNow();

        lock (state.LockObject)
        {
            state.RecentSuccesses.Enqueue(false);

            var cooldownDuration = _defaults.DefaultCooldownDuration;
            var failureThreshold = _defaults.DefaultCircuitBreakerThreshold;
            if (config is IVKAIResilienceOptions resilience)
            {
                if (resilience.CircuitBreakerBreakDuration.HasValue)
                {
                    cooldownDuration = resilience.CircuitBreakerBreakDuration.Value;
                }
                if (resilience.CircuitBreakerThreshold.HasValue)
                {
                    failureThreshold = resilience.CircuitBreakerThreshold.Value;
                }
            }

            int maxHistory = Math.Max(5, failureThreshold * 2);
            if (state.RecentSuccesses.Count > maxHistory)
            {
                state.RecentSuccesses.Dequeue();
            }

            int failures = state.RecentSuccesses.Count(s => !s);
            if (state.RecentSuccesses.Count >= failureThreshold && (double)failures / state.RecentSuccesses.Count >= 0.5)
            {
                state.CooldownUntil = now.Add(cooldownDuration);
            }
            else if (state.RecentSuccesses.Count < failureThreshold)
            {
                state.CooldownUntil = now.Add(cooldownDuration);
            }
        }
    }

    public IReadOnlyList<IVKAIProviderOptions> GetProvidersOnCooldown()
    {
        var now = _timeProvider.GetUtcNow();
        var result = new List<IVKAIProviderOptions>();

        foreach (var state in _states.Values)
        {
            lock (state.LockObject)
            {
                if (state.CooldownUntil.HasValue && state.CooldownUntil.Value > now)
                {
                    result.Add(state.Config);
                }
            }
        }
        return result;
    }

    private BreakerState GetOrCreateState(IVKAIProviderOptions config)
    {
        var key = GetProviderKey(config);
        return _states.GetOrAdd(key, _ => new BreakerState(config));
    }

    private string GetProviderKey(IVKAIProviderOptions options)
    {
        var keyStr = options.ApiKey?.ToString() ?? string.Empty;
        return $"{options.Provider}_{options.Endpoint}_{options.ModelId}_{keyStr.GetHashCode()}";
    }
}
