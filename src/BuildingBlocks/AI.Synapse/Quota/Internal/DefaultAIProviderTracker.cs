using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.Synapse.Quota.Internal;

/// <summary>
/// Default non-mutating provider availability tracker backed by <see cref="IVKCircuitBreaker"/> and <see cref="IVKAIRateLimiter"/>.
/// </summary>
// [AP.01] [CS.03]
internal sealed class DefaultAIProviderTracker : IVKAIProviderTracker
{
    private const int MaxTrackedKeys = 4096;

    private readonly IVKCircuitBreaker _circuitBreaker;
    private readonly IVKAIRateLimiter _rateLimiter;
    private readonly IVKAIMetricsCollector _metricsCollector;
    private readonly IVKAIProviderPool? _providerPool;
    private readonly ConcurrentDictionary<string, VKAIConnection> _knownConnections = new(StringComparer.OrdinalIgnoreCase);

    public DefaultAIProviderTracker(
        IVKCircuitBreaker circuitBreaker,
        IVKAIRateLimiter rateLimiter,
        IVKAIMetricsCollector metricsCollector,
        IVKAIProviderPool? providerPool = null)
    {
        _circuitBreaker = VKGuard.NotNull(circuitBreaker);
        _rateLimiter = VKGuard.NotNull(rateLimiter);
        _metricsCollector = VKGuard.NotNull(metricsCollector);
        _providerPool = providerPool;
    }

    public bool IsAvailable(VKAIConnection connection)
    {
        if (connection is null || !connection.Enabled || string.IsNullOrWhiteSpace(connection.Id))
        {
            return false;
        }

        TrackConnectionBounded(connection);

        var snapshot = _circuitBreaker.GetSnapshot(connection.Id);
        if (snapshot.State is not (VKCircuitState.Closed or VKCircuitState.HalfOpen))
        {
            return false;
        }

        return _rateLimiter.HasAvailableCapacity(connection);
    }

    public void RecordMetrics(VKAIConnection connection, int tokens, TimeSpan latency)
    {
        if (connection is null)
        {
            return;
        }

        TrackConnectionBounded(connection);
        _metricsCollector.RecordMetrics(connection, tokens, latency);
    }

    public IReadOnlyList<VKAIConnection> GetProvidersOnCooldown()
    {
        IReadOnlyList<VKAIConnection>? poolConnections = null;
        if (_providerPool is not null)
        {
            var poolTask = _providerPool.GetAvailablePoolAsync(CancellationToken.None);
            if (poolTask.IsCompletedSuccessfully && poolTask.Result.IsSuccess)
            {
                poolConnections = poolTask.Result.Value;
            }
        }

        return BuildCooldownList(poolConnections);
    }

    public async Task<IReadOnlyList<VKAIConnection>> GetProvidersOnCooldownAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VKAIConnection>? poolConnections = null;
        if (_providerPool is not null)
        {
            var poolResult = await _providerPool.GetAvailablePoolAsync(cancellationToken).ConfigureAwait(false);
            if (poolResult.IsSuccess)
            {
                poolConnections = poolResult.Value;
            }
        }

        return BuildCooldownList(poolConnections);
    }

    private List<VKAIConnection> BuildCooldownList(IReadOnlyList<VKAIConnection>? poolConnections)
    {
        var byId = new Dictionary<string, VKAIConnection>(StringComparer.OrdinalIgnoreCase);

        if (poolConnections is not null)
        {
            foreach (var conn in poolConnections)
            {
                if (!string.IsNullOrWhiteSpace(conn.Id))
                {
                    byId[conn.Id] = conn;
                }
            }
        }

        foreach (var pair in _knownConnections)
        {
            byId.TryAdd(pair.Key, pair.Value);
        }

        var result = new List<VKAIConnection>();
        var addedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var conn in byId.Values)
        {
            var snapshot = _circuitBreaker.GetSnapshot(conn.Id);
            if (snapshot.State is VKCircuitState.Open or VKCircuitState.Isolated && addedIds.Add(conn.Id))
            {
                result.Add(conn);
            }
        }

        foreach (var key in _circuitBreaker.GetKeysOnCooldown())
        {
            if (string.IsNullOrWhiteSpace(key) || !addedIds.Add(key))
            {
                continue;
            }

            result.Add(byId.TryGetValue(key, out var conn)
                ? conn
                : new VKAIConnection { Id = key, Name = key });
        }

        return result;
    }

    private void TrackConnectionBounded(VKAIConnection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.Id))
        {
            return;
        }

        if (_knownConnections.ContainsKey(connection.Id))
        {
            _knownConnections[connection.Id] = connection;
            return;
        }

        if (_knownConnections.Count >= MaxTrackedKeys)
        {
            foreach (var key in _knownConnections.Keys)
            {
                if (_knownConnections.TryRemove(key, out _))
                {
                    break;
                }
            }
        }

        _knownConnections[connection.Id] = connection;
    }
}
