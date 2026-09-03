using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VK.Blocks.Core;
using VK.Blocks.Resilience.Diagnostics.Internal;

namespace VK.Blocks.Resilience.CircuitBreaker.Internal;

// [AP.01] sealed
internal sealed class LocalCircuitBreaker : IVKCircuitBreaker
{
    private sealed class BreakerState
    {
        public VKCircuitState State { get; set; } = VKCircuitState.Closed;
        public DateTimeOffset? CooldownUntil { get; set; }
        public int HalfOpenTrialInFlight { get; set; }
        public Queue<(DateTimeOffset Timestamp, bool Success)> RecentExecutions { get; } = new();
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

    public VKCircuitState GetState(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        var state = _states.GetOrAdd(key, _ => new BreakerState());
        var now = _timeProvider.GetUtcNow();

        lock (state.LockObject)
        {
            if (state.State == VKCircuitState.Open && state.CooldownUntil.HasValue && now >= state.CooldownUntil.Value)
            {
                state.State = VKCircuitState.HalfOpen;
                state.HalfOpenTrialInFlight = 0;
            }

            return state.State;
        }
    }

    public bool IsAllowed(string key)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        var state = _states.GetOrAdd(key, _ => new BreakerState());
        var now = _timeProvider.GetUtcNow();

        lock (state.LockObject)
        {
            if (state.State == VKCircuitState.Open)
            {
                if (state.CooldownUntil.HasValue && now >= state.CooldownUntil.Value)
                {
                    // Transition to HalfOpen on timeout expiration
                    state.State = VKCircuitState.HalfOpen;
                    state.HalfOpenTrialInFlight = 1;
                    return true;
                }

                return false;
            }

            if (state.State == VKCircuitState.HalfOpen)
            {
                if (state.HalfOpenTrialInFlight < _options.PermittedNumberOfCallsInHalfOpenState)
                {
                    state.HalfOpenTrialInFlight++;
                    return true;
                }

                return false;
            }

            return true;
        }
    }

    public void RecordSuccess(string key, int? failureThreshold = null, Action<string>? onReset = null)
    {
        VKGuard.NotNullOrWhiteSpace(key);

        var state = _states.GetOrAdd(key, _ => new BreakerState());
        var now = _timeProvider.GetUtcNow();
        int effectiveThreshold = failureThreshold ?? _options.MinimumThroughput;

        lock (state.LockObject)
        {
            if (state.State == VKCircuitState.HalfOpen)
            {
                // Probation successful -> transition back to Closed
                state.State = VKCircuitState.Closed;
                state.CooldownUntil = null;
                state.HalfOpenTrialInFlight = 0;
                state.RecentExecutions.Clear();
                ResilienceDiagnostics.RecordStrategyExecution("circuit_breaker_reset", true);
                onReset?.Invoke(key);
                return;
            }

            CleanExpiredExecutions(state, now);
            state.RecentExecutions.Enqueue((now, true));
            state.CooldownUntil = null;
        }
    }

    public void RecordFailure(
        string key,
        Exception ex,
        TimeSpan? cooldownDuration = null,
        int? failureThreshold = null,
        double? failureRatio = null,
        Action<string, TimeSpan>? onBreak = null)
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
            if (state.State == VKCircuitState.HalfOpen)
            {
                // Failed during probation -> immediate trip back to Open
                TripCircuit(state, key, now, effectiveCooldown, onBreak);
                return;
            }

            CleanExpiredExecutions(state, now);
            state.RecentExecutions.Enqueue((now, false));

            int totalCalls = state.RecentExecutions.Count;
            int failures = state.RecentExecutions.Count(e => !e.Success);

            if (totalCalls >= effectiveThreshold && (double)failures / totalCalls >= effectiveRatio)
            {
                TripCircuit(state, key, now, effectiveCooldown, onBreak);
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
                if (pair.Value.State == VKCircuitState.Open &&
                    pair.Value.CooldownUntil.HasValue &&
                    pair.Value.CooldownUntil.Value > now)
                {
                    result.Add(pair.Key);
                }
            }
        }
        return result;
    }

    private static void TripCircuit(
        BreakerState state,
        string key,
        DateTimeOffset now,
        TimeSpan effectiveCooldown,
        Action<string, TimeSpan>? onBreak)
    {
        state.State = VKCircuitState.Open;
        state.CooldownUntil = now.Add(effectiveCooldown);
        state.HalfOpenTrialInFlight = 0;
        state.RecentExecutions.Clear();
        ResilienceDiagnostics.RecordStrategyExecution("circuit_breaker_tripped", false);
        onBreak?.Invoke(key, effectiveCooldown);
    }

    private void CleanExpiredExecutions(BreakerState state, DateTimeOffset now)
    {
        var windowStart = now.Subtract(_options.SamplingDuration);
        while (state.RecentExecutions.Count > 0 && state.RecentExecutions.Peek().Timestamp < windowStart)
        {
            state.RecentExecutions.Dequeue();
        }
    }
}
