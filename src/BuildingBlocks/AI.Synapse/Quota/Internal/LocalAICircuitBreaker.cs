using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VK.Blocks.Core;
using VK.Blocks.Resilience;

namespace VK.Blocks.AI.Synapse.Internal;

// [AP.01] sealed
internal sealed class LocalAICircuitBreaker : IVKAICircuitBreaker
{
    private readonly IVKCircuitBreaker _circuitBreaker;
    private readonly VKQuotaOptions _defaults;
    private readonly ConcurrentDictionary<string, VKAIConnection> _knownConnections = new(StringComparer.OrdinalIgnoreCase);

    public LocalAICircuitBreaker(
        IVKCircuitBreaker circuitBreaker,
        VKQuotaOptions defaults)
    {
        _circuitBreaker = VKGuard.NotNull(circuitBreaker);
        _defaults = VKGuard.NotNull(defaults);
    }

    public bool IsAllowed(VKAIConnection connection)
    {
        if (connection is null)
            return false;

        var key = GetConnectionKey(connection);
        _knownConnections.TryAdd(key, connection);
        return _circuitBreaker.IsAllowed(key);
    }

    public void RecordSuccess(VKAIConnection connection)
    {
        if (connection is null)
            return;

        var key = GetConnectionKey(connection);
        _knownConnections.TryAdd(key, connection);
        _circuitBreaker.RecordSuccess(key, _defaults.DefaultCircuitBreakerThreshold);
    }

    public void RecordFailure(VKAIConnection connection, Exception ex)
    {
        if (connection is null)
            return;

        var key = GetConnectionKey(connection);
        _knownConnections.TryAdd(key, connection);

        _circuitBreaker.RecordFailure(
            key,
            ex,
            _defaults.DefaultCooldownDuration,
            _defaults.DefaultCircuitBreakerThreshold,
            0.5);
    }

    public IReadOnlyList<VKAIConnection> GetProvidersOnCooldown()
    {
        var cooldownKeys = _circuitBreaker.GetKeysOnCooldown();
        var result = new List<VKAIConnection>();

        foreach (var key in cooldownKeys)
        {
            if (_knownConnections.TryGetValue(key, out var connection))
            {
                result.Add(connection);
            }
        }

        return result;
    }

    private static string GetConnectionKey(VKAIConnection connection)
    {
        return $"{connection.TenantId}_{connection.Id}";
    }
}
