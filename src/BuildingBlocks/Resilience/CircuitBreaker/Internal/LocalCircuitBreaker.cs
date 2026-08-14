using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.CircuitBreaker.Internal;

// [AP.01] sealed
internal sealed class LocalCircuitBreaker : IVKCircuitBreaker
{
    private sealed class BreakerState
    {
        public DateTimeOffset? CooldownUntil { get; set; }
        public Queue<bool> RecentSuccesses { get; } = new();
        public object LockObject { get; } = new();
    }

    private readonly ConcurrentDictionary<string, BreakerState> _states = new();
    private readonly VKCircuitBreakerOptions _options;
    private readonly TimeProvider _timeProvider;

    public LocalCircuitBreaker(VKCircuitBreakerOptions options, TimeProvider? timeProvider = null)
    {
        _options = VKGuard.NotNull(options);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public bool IsAllowed(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        var state = _states.GetOrAdd(key, _ => new BreakerState());
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

    public void RecordSuccess(string key, int? failureThreshold = null)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        var state = _states.GetOrAdd(key, _ => new BreakerState());
        int effectiveThreshold = failureThreshold ?? _options.MinimumThroughput;

        lock (state.LockObject)
        {
            state.RecentSuccesses.Enqueue(true);
            int maxHistory = Math.Max(5, effectiveThreshold * 2);
            if (state.RecentSuccesses.Count > maxHistory)
            {
                state.RecentSuccesses.Dequeue();
            }
            state.CooldownUntil = null;
        }
    }

    public void RecordFailure(
        string key,
        Exception ex,
        TimeSpan? cooldownDuration = null,
        int? failureThreshold = null,
        double? failureRatio = null)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        VKGuard.NotNull(ex);

        var state = _states.GetOrAdd(key, _ => new BreakerState());
        var now = _timeProvider.GetUtcNow();
        var effectiveCooldown = cooldownDuration ?? _options.DurationOfBreak;
        int effectiveThreshold = failureThreshold ?? _options.MinimumThroughput;
        double effectiveRatio = failureRatio ?? _options.FailureThreshold;

        lock (state.LockObject)
        {
            state.RecentSuccesses.Enqueue(false);
            int maxHistory = Math.Max(5, effectiveThreshold * 2);
            if (state.RecentSuccesses.Count > maxHistory)
            {
                state.RecentSuccesses.Dequeue();
            }

            int failures = state.RecentSuccesses.Count(s => !s);
            if (state.RecentSuccesses.Count >= effectiveThreshold && (double)failures / state.RecentSuccesses.Count >= effectiveRatio)
            {
                state.CooldownUntil = now.Add(effectiveCooldown);
            }
            else if (state.RecentSuccesses.Count < effectiveThreshold)
            {
                state.CooldownUntil = now.Add(effectiveCooldown);
            }
        }
    }

    public IReadOnlyList<string> GetKeysOnCooldown()
    {
        var now = _timeProvider.GetUtcNow();
        var result = new List<string>();

        foreach (var pair in _states)
        {
            lock (pair.Value.LockObject)
            {
                if (pair.Value.CooldownUntil.HasValue && pair.Value.CooldownUntil.Value > now)
                {
                    result.Add(pair.Key);
                }
            }
        }
        return result;
    }
}
