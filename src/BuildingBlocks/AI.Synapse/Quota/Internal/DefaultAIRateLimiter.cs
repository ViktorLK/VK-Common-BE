using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Quota.Internal;

/// <summary>
/// Bounded per-connection concurrency and RPM rate limiter backed by <see cref="System.Threading.RateLimiting"/> leases.
/// </summary>
// [AP.01] [CS.03]
internal sealed class DefaultAIRateLimiter : IVKAIRateLimiter, IDisposable
{
    private const int MaxTrackedKeys = 4096;

    private readonly VKQuotaOptions _defaults;
    private readonly ConcurrentDictionary<string, LimiterEntry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<LimiterEntry> _retiredEntries = [];
    private readonly object _syncRoot = new();
    private bool _disposed;

    public DefaultAIRateLimiter(VKQuotaOptions defaults)
    {
        _defaults = VKGuard.NotNull(defaults);
    }

    public bool HasAvailableCapacity(VKAIConnection connection)
    {
        if (connection is null || string.IsNullOrWhiteSpace(connection.Id) || Volatile.Read(ref _disposed))
        {
            return false;
        }

        if (!_entries.TryGetValue(connection.Id, out var entry))
        {
            return true;
        }

        (int maxConcurrency, int rpm) = ResolveLimits(connection);
        if (!entry.Matches(maxConcurrency, rpm))
        {
            if (entry.IsIdle())
            {
                return true;
            }

            RateLimiterStatistics? concurrencyStats = entry.Concurrency.GetStatistics();
            long inFlight = concurrencyStats is not null
                ? Math.Max(0, entry.MaxConcurrency - concurrencyStats.CurrentAvailablePermits)
                : 0;
            return inFlight < maxConcurrency;
        }

        RateLimiterStatistics? stats = entry.EffectiveLimiter.GetStatistics();
        return stats is null || stats.CurrentAvailablePermits > 0;
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownership of the acquired capacity lease is transferred to the caller.")]
    public ValueTask<IVKAICapacityLease> AcquireLeaseAsync(
        VKAIConnection connection,
        string? reservationId = null,
        CancellationToken cancellationToken = default)
    {
        // [AP.01] boundary guard
        VKGuard.NotNull(connection);
        VKGuard.NotNullOrWhiteSpace(connection.Id);
        cancellationToken.ThrowIfCancellationRequested();
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed), this);

        var entry = GetOrCreateEntry(connection);

        var lease = entry.EffectiveLimiter.AttemptAcquire(1);
        return ValueTask.FromResult<IVKAICapacityLease>(CreateCapacityLease(lease, connection.Id, reservationId));
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

            foreach (var entry in _entries.Values)
            {
                entry.Dispose();
            }

            _entries.Clear();

            for (int i = 0; i < _retiredEntries.Count; i++)
            {
                _retiredEntries[i].Dispose();
            }

            _retiredEntries.Clear();
        }
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Failed leases are disposed immediately; acquired leases transfer ownership to RateLimitingCapacityLease.")]
    private static RateLimitingCapacityLease CreateCapacityLease(
        RateLimitLease lease,
        string connectionId,
        string? reservationId)
    {
        if (lease.IsAcquired)
        {
            return new RateLimitingCapacityLease(lease, connectionId, reservationId);
        }

        TimeSpan? retryAfter = lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan ra) && ra > TimeSpan.Zero
            ? ra
            : null;
        lease.Dispose();

        return new RateLimitingCapacityLease(lease: null, connectionId, reservationId, retryAfter);
    }

    private (int MaxConcurrency, int Rpm) ResolveLimits(VKAIConnection connection)
    {
        int maxConcurrency = connection.MaxConcurrency > 0
            ? connection.MaxConcurrency
            : Math.Max(1, _defaults.DefaultMaxConcurrency);
        int rpm = connection.RpmLimit ?? _defaults.DefaultRequestsPerMinute;
        return (maxConcurrency, Math.Max(0, rpm));
    }

    private LimiterEntry GetOrCreateEntry(VKAIConnection connection)
    {
        (int maxConcurrency, int rpm) = ResolveLimits(connection);

        if (_entries.TryGetValue(connection.Id, out var existing) &&
            existing.Matches(maxConcurrency, rpm))
        {
            return existing;
        }

        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            DrainIdleRetiredEntriesLocked();

            if (_entries.TryGetValue(connection.Id, out existing))
            {
                if (existing.Matches(maxConcurrency, rpm))
                {
                    return existing;
                }

                var updated = new LimiterEntry(maxConcurrency, rpm);
                _entries[connection.Id] = updated;

                if (existing.IsIdle())
                {
                    existing.Dispose();
                }
                else
                {
                    _retiredEntries.Add(existing);
                }

                return updated;
            }

            if (_entries.Count >= MaxTrackedKeys)
            {
                EvictIdleEntryLocked();
            }

            var created = new LimiterEntry(maxConcurrency, rpm);
            _entries[connection.Id] = created;
            return created;
        }
    }

    private void EvictIdleEntryLocked()
    {
        foreach (var pair in _entries)
        {
            if (pair.Value.IsIdle() && _entries.TryRemove(KeyValuePair.Create(pair.Key, pair.Value)))
            {
                pair.Value.Dispose();
                return;
            }
        }
    }

    private void DrainIdleRetiredEntriesLocked()
    {
        for (int i = _retiredEntries.Count - 1; i >= 0; i--)
        {
            var entry = _retiredEntries[i];
            if (entry.IsIdle())
            {
                _retiredEntries.RemoveAt(i);
                entry.Dispose();
            }
        }
    }

    private sealed class LimiterEntry : IDisposable
    {
        public LimiterEntry(int maxConcurrency, int rpm)
        {
            MaxConcurrency = maxConcurrency;
            Rpm = rpm;

            Concurrency = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
            {
                PermitLimit = maxConcurrency,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });

            if (rpm > 0)
            {
                TokenBucket = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
                {
                    TokenLimit = rpm,
                    TokensPerPeriod = rpm,
                    ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                    AutoReplenishment = true,
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                });
                EffectiveLimiter = RateLimiter.CreateChained(Concurrency, TokenBucket);
            }
            else
            {
                EffectiveLimiter = Concurrency;
            }
        }

        public int MaxConcurrency { get; }
        public int Rpm { get; }
        public ConcurrencyLimiter Concurrency { get; }
        public TokenBucketRateLimiter? TokenBucket { get; }
        public RateLimiter EffectiveLimiter { get; }

        public bool Matches(int maxConcurrency, int rpm) =>
            MaxConcurrency == maxConcurrency && Rpm == rpm;

        public bool IsIdle()
        {
            RateLimiterStatistics? stats = Concurrency.GetStatistics();
            return stats is not null
                && stats.CurrentQueuedCount == 0
                && stats.CurrentAvailablePermits >= MaxConcurrency;
        }

        public void Dispose()
        {
            if (!ReferenceEquals(EffectiveLimiter, Concurrency))
            {
                EffectiveLimiter.Dispose();
            }

            TokenBucket?.Dispose();
            Concurrency.Dispose();
        }
    }

    private sealed class RateLimitingCapacityLease : IVKAICapacityLease
    {
        private RateLimitLease? _lease;

        public RateLimitingCapacityLease(
            RateLimitLease? lease,
            string connectionId,
            string? reservationId,
            TimeSpan? retryAfter = null)
        {
            _lease = lease;
            IsAcquired = lease is not null && lease.IsAcquired;
            ConnectionId = connectionId;
            ReservationId = reservationId;
            RetryAfter = retryAfter;
        }

        public bool IsAcquired { get; }
        public string ConnectionId { get; }
        public string? ReservationId { get; }
        public TimeSpan? RetryAfter { get; }

        public void Dispose()
        {
            Interlocked.Exchange(ref _lease, null)?.Dispose();
        }
    }
}
