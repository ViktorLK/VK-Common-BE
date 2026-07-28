using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Engram.Structured.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Structured.Internal;

/// <summary>
/// Default in-memory implementation of <see cref="IVKStructuredMemoryStore"/>.
/// Stores deterministic key-value facts completely isolated from Decay and Pruning.
/// Integrates TenantId scope isolation, schema/type validation, GDPR explicit deletion, and sensitivity masking.
/// </summary>
internal sealed class InMemoryStructuredMemoryStore : IVKStructuredMemoryStore
{
    private readonly ConcurrentDictionary<string, VKStructuredFact> _facts = new();
    private readonly IVKUserContext? _userContext;
    private readonly IVKFactSensitivityPolicy _sensitivityPolicy;
    private readonly IVKFactCapacityPolicy _capacityPolicy;
    private readonly TimeProvider _timeProvider;
    private readonly VKStructuredOptions _options;
    private readonly ILogger<InMemoryStructuredMemoryStore> _logger;

    public InMemoryStructuredMemoryStore(
        ILogger<InMemoryStructuredMemoryStore> logger,
        VKStructuredOptions? options = null,
        IVKUserContext? userContext = null,
        IVKFactSensitivityPolicy? sensitivityPolicy = null,
        IVKFactCapacityPolicy? capacityPolicy = null,
        TimeProvider? timeProvider = null)
    {
        _logger = VKGuard.NotNull(logger);
        _options = options ?? new VKStructuredOptions();
        _userContext = userContext;
        _sensitivityPolicy = sensitivityPolicy ?? new DefaultFactSensitivityPolicy();
        _capacityPolicy = capacityPolicy ?? new DefaultFactCapacityPolicy(_options);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<VKResult> StoreFactAsync(
        string key,
        object value,
        Type? expectedType = null,
        bool isSensitive = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(key);
        VKGuard.NotNull(value);

        // Schema / Type validation
        if (expectedType is not null && !expectedType.IsInstanceOfType(value))
        {
            try
            {
                value = Convert.ChangeType(value, expectedType);
            }
            catch (Exception)
            {
                return VKResult.Failure(VKStructuredErrors.SchemaValidationFailed);
            }
        }

        var currentTenantId = _userContext?.TenantId;
        var scopedKey = BuildScopedKey(key, currentTenantId);
        var now = _timeProvider.GetUtcNow();

        if (_facts.TryGetValue(scopedKey, out var existingFact))
        {
            // Conflict Resolution (Last-Write-Wins with diagnostic audit)
            string oldMasked = existingFact.IsSensitive ? _sensitivityPolicy.MaskSensitiveValue(existingFact.Value) : existingFact.Value.ToString() ?? "";
            string newMasked = isSensitive ? _sensitivityPolicy.MaskSensitiveValue(value) : value.ToString() ?? "";

            _logger.FactConflictResolved(key, oldMasked, newMasked, currentTenantId?.Value.ToString());

            var updatedFact = existingFact with
            {
                Value = value,
                ExpectedType = expectedType ?? existingFact.ExpectedType,
                UpdatedAt = now,
                IsSensitive = isSensitive
            };
            _facts[scopedKey] = updatedFact;
        }
        else
        {
            // Capacity limit check via IVKFactCapacityPolicy for new keys
            var tenantPrefix = currentTenantId is not null ? $"{currentTenantId.Value}:" : "global:";
            int currentCount = _facts.Keys.Count(k => k.StartsWith(tenantPrefix, StringComparison.OrdinalIgnoreCase));
            var capacityResult = await _capacityPolicy.ValidateCapacityAsync(currentTenantId, currentCount, cancellationToken).ConfigureAwait(false);
            if (capacityResult.IsFailure)
            {
                return capacityResult;
            }

            var newFact = new VKStructuredFact
            {
                Key = key,
                Value = value,
                ExpectedType = expectedType,
                TenantId = currentTenantId,
                StoredAt = now,
                IsSensitive = isSensitive
            };
            _facts[scopedKey] = newFact;
        }

        _logger.FactStored(key, currentTenantId?.Value.ToString(), isSensitive);
        return VKResult.Success();
    }

    public Task<VKResult<T>> GetFactAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(key);

        var currentTenantId = _userContext?.TenantId;
        var scopedKey = BuildScopedKey(key, currentTenantId);

        if (_facts.TryGetValue(scopedKey, out var fact))
        {
            if (fact.Value is T typedValue)
            {
                return Task.FromResult(VKResult.Success(typedValue));
            }
            try
            {
                var converted = (T)Convert.ChangeType(fact.Value, typeof(T));
                return Task.FromResult(VKResult.Success(converted));
            }
            catch (Exception)
            {
                _logger.FactTypeMismatch(key, typeof(T).Name, fact.Value.GetType().Name);
                return Task.FromResult(VKResult.Failure<T>(VKStructuredErrors.TypeMismatch));
            }
        }

        return Task.FromResult(VKResult.Failure<T>(VKStructuredErrors.NotFound));
    }

    public Task<VKResult<bool>> HasFactAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(key);

        var currentTenantId = _userContext?.TenantId;
        var scopedKey = BuildScopedKey(key, currentTenantId);

        bool exists = _facts.ContainsKey(scopedKey);
        return Task.FromResult(VKResult.Success(exists));
    }

    public Task<VKResult> RemoveFactAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(key);

        var currentTenantId = _userContext?.TenantId;
        var scopedKey = BuildScopedKey(key, currentTenantId);

        if (_facts.TryRemove(scopedKey, out _))
        {
            _logger.FactRemoved(key, currentTenantId?.Value.ToString());
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<int>> RemoveAllFactsAsync(string? prefix = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentTenantId = _userContext?.TenantId;
        var tenantPrefix = currentTenantId is not null ? $"{currentTenantId.Value}:" : "global:";
        var targetPrefix = string.IsNullOrWhiteSpace(prefix) ? tenantPrefix : $"{tenantPrefix}{prefix}";

        var keysToRemove = _facts.Keys.Where(k => k.StartsWith(targetPrefix, StringComparison.OrdinalIgnoreCase)).ToList();
        int removedCount = 0;

        foreach (var key in keysToRemove)
        {
            if (_facts.TryRemove(key, out var fact))
            {
                removedCount++;
                _logger.FactRemoved(fact.Key, currentTenantId?.Value.ToString());
            }
        }

        return Task.FromResult(VKResult.Success(removedCount));
    }

    public Task<VKResult<IEnumerable<string>>> ListKeysAsync(string? prefix = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentTenantId = _userContext?.TenantId;
        var tenantPrefix = currentTenantId is not null ? $"{currentTenantId.Value}:" : "global:";

        var keys = _facts.Values
            .Where(f => currentTenantId is null || f.TenantId == currentTenantId)
            .Select(f => f.Key);

        if (!string.IsNullOrWhiteSpace(prefix))
        {
            keys = keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(VKResult.Success<IEnumerable<string>>(keys.ToList()));
    }

    private static string BuildScopedKey(string key, VKTenantId? tenantId)
    {
        return tenantId is not null ? $"{tenantId.Value}:{key}" : $"global:{key}";
    }
}
