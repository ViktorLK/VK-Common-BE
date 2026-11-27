using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Quota.Internal;

/// <summary>
/// Thread-safe bounded token budget manager supporting pre-reservation, credit refund on settlement/release, and TTL pruning.
/// </summary>
// [AP.01] [CS.03] [CS.06]
internal sealed class DefaultAITokenBudgetManager : IVKAITokenBudgetManager, IDisposable
{
    private const int MaxTrackedKeys = 4096;
    private static readonly TimeSpan ReplenishmentPeriod = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan StaleReservationTtl = TimeSpan.FromMinutes(10);

    private readonly VKQuotaOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ConcurrentDictionary<string, TokenBucketRateLimiter> _limiters = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, long> _refundCredits = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ReservationEntry> _reservations = new(StringComparer.Ordinal);
    private readonly object _syncRoot = new();
    private bool _disposed;

    public DefaultAITokenBudgetManager(VKQuotaOptions options, TimeProvider? timeProvider = null)
    {
        _options = VKGuard.NotNull(options);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public Task<VKResult> ReserveTokensAsync(
        VKAIScopeKey scopeKey,
        string invocationId,
        int estimatedTokens,
        CancellationToken cancellationToken = default)
    {
        // [AP.01] boundary guard
        VKGuard.NotNullOrWhiteSpace(invocationId);
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed), this);

        if (!_options.EnableTokenBudget || estimatedTokens <= 0)
        {
            return Task.FromResult(VKResult.Success());
        }

        DateTimeOffset now = _timeProvider.GetUtcNow();
        string scopeId = BuildScopeKey(scopeKey);
        int maxTpm = GetMaxTpm();

        int drawnFromCredit;
        TokenBucketRateLimiter? limiter = null;

        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            PruneStaleReservationsLocked(now);

            if (_reservations.ContainsKey(invocationId))
            {
                return Task.FromResult(VKResult.Success());
            }

            drawnFromCredit = DrawRefundCreditLocked(scopeId, estimatedTokens);
            int remaining = estimatedTokens - drawnFromCredit;

            if (remaining > 0)
            {
                if (remaining > maxTpm)
                {
                    AddRefundCreditLocked(scopeId, drawnFromCredit, maxTpm);
                    return Task.FromResult(VKResult.Failure(VKAISynapseErrors.QuotaExceeded));
                }

                limiter = GetOrCreateLimiterLocked(scopeId, maxTpm);
            }
        }

        int remainderToAcquire = estimatedTokens - drawnFromCredit;
        if (remainderToAcquire > 0)
        {
            RateLimitLease? lease = null;
            try
            {
                lease = limiter!.AttemptAcquire(remainderToAcquire);

                if (!lease.IsAcquired)
                {
                    TimeSpan retryAfter = ResolveRetryAfter(lease, limiter, remainderToAcquire, maxTpm);
                    RefundCredit(scopeId, drawnFromCredit, maxTpm);
                    return Task.FromResult(VKResult.Failure(VKAISynapseErrors.CreateQuotaExceeded(retryAfter)));
                }
            }
            catch
            {
                RefundCredit(scopeId, drawnFromCredit, maxTpm);
                throw;
            }
            finally
            {
                lease?.Dispose();
            }
        }

        lock (_syncRoot)
        {
            if (_disposed)
            {
                return Task.FromResult(VKResult.Success());
            }

            if (_reservations.ContainsKey(invocationId))
            {
                AddRefundCreditLocked(scopeId, estimatedTokens, maxTpm);
                return Task.FromResult(VKResult.Success());
            }

            EnsureReservationCapacityLocked(now);
            _reservations[invocationId] = new ReservationEntry(scopeId, estimatedTokens, _timeProvider.GetUtcNow());
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> SettleTokensAsync(
        VKAIScopeKey scopeKey,
        string invocationId,
        int actualTokens,
        CancellationToken cancellationToken = default)
    {
        // [AP.01] boundary guard
        VKGuard.NotNullOrWhiteSpace(invocationId);
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed), this);

        bool hadReservation;
        ReservationEntry reservation;
        int normalizedActual = Math.Max(0, actualTokens);
        int maxTpm = GetMaxTpm();
        string scopeId;
        int reserved;
        TokenBucketRateLimiter? limiter = null;

        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            hadReservation = _reservations.TryRemove(invocationId, out reservation);
            if (!_options.EnableTokenBudget)
            {
                return Task.FromResult(VKResult.Success());
            }

            reserved = hadReservation ? reservation.ReservedTokens : 0;
            scopeId = hadReservation && !string.IsNullOrEmpty(reservation.ScopeId)
                ? reservation.ScopeId
                : BuildScopeKey(scopeKey);

            int delta = normalizedActual - reserved;
            if (delta < 0)
            {
                AddRefundCreditLocked(scopeId, reserved - normalizedActual, maxTpm);
                return Task.FromResult(VKResult.Success());
            }

            if (delta == 0)
            {
                return Task.FromResult(VKResult.Success());
            }

            int drawn = DrawRefundCreditLocked(scopeId, delta);
            int remainingDelta = delta - drawn;
            if (remainingDelta <= 0)
            {
                return Task.FromResult(VKResult.Success());
            }

            limiter = GetOrCreateLimiterLocked(scopeId, maxTpm);
            int toAcquire = Math.Min(remainingDelta, maxTpm);
            using var _ = limiter.AttemptAcquire(toAcquire);
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> ReleaseTokensAsync(
        VKAIScopeKey scopeKey,
        string invocationId,
        CancellationToken cancellationToken = default)
    {
        // [AP.01] boundary guard
        VKGuard.NotNullOrWhiteSpace(invocationId);
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed), this);

        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_reservations.TryRemove(invocationId, out var reservation) &&
                _options.EnableTokenBudget &&
                reservation.ReservedTokens > 0)
            {
                string scopeId = !string.IsNullOrEmpty(reservation.ScopeId)
                    ? reservation.ScopeId
                    : BuildScopeKey(scopeKey);
                AddRefundCreditLocked(scopeId, reservation.ReservedTokens, GetMaxTpm());
            }
        }

        return Task.FromResult(VKResult.Success());
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (_disposed)
            {
                return;
            }

            Volatile.Write(ref _disposed, true);

            foreach (var limiter in _limiters.Values)
            {
                limiter.Dispose();
            }

            _limiters.Clear();
            _refundCredits.Clear();
            _reservations.Clear();
        }
    }

    private int GetMaxTpm() => _options.DefaultTokensPerMinute > 0 ? _options.DefaultTokensPerMinute : 100_000;

    private static string BuildScopeKey(VKAIScopeKey scopeKey) => scopeKey.Value ?? string.Empty;

    private void RefundCredit(string scopeId, int amount, int maxTpm)
    {
        if (amount <= 0)
        {
            return;
        }

        lock (_syncRoot)
        {
            if (!_disposed)
            {
                AddRefundCreditLocked(scopeId, amount, maxTpm);
            }
        }
    }

    private int DrawRefundCreditLocked(string scopeId, int requested)
    {
        if (requested <= 0 || !_refundCredits.TryGetValue(scopeId, out long available) || available <= 0)
        {
            return 0;
        }

        int drawn = (int)Math.Min(available, requested);
        long remaining = available - drawn;
        if (remaining <= 0)
        {
            _refundCredits.TryRemove(scopeId, out _);
        }
        else
        {
            _refundCredits[scopeId] = remaining;
        }

        return drawn;
    }

    private void AddRefundCreditLocked(string scopeId, int amount, int maxTpm)
    {
        if (amount <= 0)
        {
            return;
        }

        if (!_refundCredits.ContainsKey(scopeId) && _refundCredits.Count >= MaxTrackedKeys)
        {
            foreach (var key in _refundCredits.Keys)
            {
                if (_refundCredits.TryRemove(key, out _))
                {
                    break;
                }
            }
        }

        long current = _refundCredits.GetValueOrDefault(scopeId);
        long updated = Math.Min(maxTpm, current + amount);
        if (updated > 0)
        {
            _refundCredits[scopeId] = updated;
        }
    }

    private TokenBucketRateLimiter GetOrCreateLimiterLocked(string scopeId, int maxTpm)
    {
        if (_limiters.TryGetValue(scopeId, out var existing))
        {
            return existing;
        }

        if (_limiters.Count >= MaxTrackedKeys)
        {
            EvictIdleLimiterLocked();
        }

        var created = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = maxTpm,
            TokensPerPeriod = maxTpm,
            ReplenishmentPeriod = ReplenishmentPeriod,
            AutoReplenishment = true,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });

        _limiters[scopeId] = created;
        return created;
    }

    private static TimeSpan ResolveRetryAfter(
        RateLimitLease lease,
        TokenBucketRateLimiter limiter,
        int requiredTokens,
        int maxTpm)
    {
        if (lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter) && retryAfter > TimeSpan.Zero)
        {
            return retryAfter;
        }

        RateLimiterStatistics? stats = limiter.GetStatistics();
        long available = stats is not null ? Math.Max(0, stats.CurrentAvailablePermits) : 0;
        long deficit = Math.Max(1, requiredTokens - available);
        double ratio = Math.Clamp((double)deficit / Math.Max(1, maxTpm), 0.0, 1.0);
        double estimatedMs = Math.Ceiling(ratio * ReplenishmentPeriod.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(Math.Max(1, estimatedMs));
    }

    private void EvictIdleLimiterLocked()
    {
        foreach (var pair in _limiters)
        {
            RateLimiterStatistics? stats = pair.Value.GetStatistics();
            if (stats is null || stats.CurrentQueuedCount == 0)
            {
                if (_limiters.TryRemove(KeyValuePair.Create(pair.Key, pair.Value)))
                {
                    pair.Value.Dispose();
                    return;
                }
            }
        }
    }

    private void PruneStaleReservationsLocked(DateTimeOffset now)
    {
        foreach (var pair in _reservations)
        {
            if (now - pair.Value.CreatedAt > StaleReservationTtl)
            {
                _reservations.TryRemove(pair.Key, out _);
            }
        }
    }

    private void EnsureReservationCapacityLocked(DateTimeOffset now)
    {
        if (_reservations.Count < MaxTrackedKeys)
        {
            return;
        }

        PruneStaleReservationsLocked(now);
        if (_reservations.Count < MaxTrackedKeys)
        {
            return;
        }

        string? oldestKey = null;
        DateTimeOffset oldestTime = DateTimeOffset.MaxValue;
        foreach (var pair in _reservations)
        {
            if (pair.Value.CreatedAt < oldestTime)
            {
                oldestTime = pair.Value.CreatedAt;
                oldestKey = pair.Key;
            }
        }

        if (oldestKey is not null)
        {
            _reservations.TryRemove(oldestKey, out _);
        }
    }

    private readonly record struct ReservationEntry(string ScopeId, int ReservedTokens, DateTimeOffset CreatedAt);
}
