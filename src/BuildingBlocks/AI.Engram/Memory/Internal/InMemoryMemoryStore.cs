using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Engram.Memory.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Memory.Internal;

/// <summary>
/// Default concrete implementation of <see cref="IVKMemoryStore"/>.
/// Acts as pure Source of Truth storage for memory entries (CRUD only).
/// </summary>
internal sealed class InMemoryMemoryStore : IVKMemoryStore
{
    private readonly ConcurrentDictionary<VKMemoryId, VKMemoryEntry> _store = new();
    private readonly IVKTokenCounter _tokenCounter;
    private readonly IVKTenantCoordinate _tenantCoordinate;
    private readonly ILogger<InMemoryMemoryStore> _logger;

    public InMemoryMemoryStore(
        IVKTokenCounter tokenCounter,
        ILogger<InMemoryMemoryStore> logger,
        IVKTenantCoordinate tenantCoordinate)
    {
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _logger = VKGuard.NotNull(logger);
        _tenantCoordinate = VKGuard.NotNull(tenantCoordinate);
    }

    public Task<VKResult> UpsertAsync(
        VKMemoryEntry entry,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entry);

        if (entry.TenantId is null)
        {
            entry = entry with { TenantId = _tenantCoordinate.TenantId };
        }

        if (entry.Category == VKMemoryCategory.ShortTerm && !entry.Metadata.ContainsKey("TokenCount"))
        {
            var tokenCount = _tokenCounter.CountTokens(entry.Content);
            var updatedMetadata = new Dictionary<string, string>(entry.Metadata)
            {
                ["TokenCount"] = tokenCount.ToString()
            };
            entry = entry with { Metadata = updatedMetadata };
        }

        _store[entry.Id] = entry;
        _logger.MemoryEntrySaved(entry.Id.ToString());

        return Task.FromResult(VKResult.Success());
    }

    public async Task<VKResult> UpsertBatchAsync(
        IEnumerable<VKMemoryEntry> entries,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(entries);

        foreach (var entry in entries)
        {
            var result = await UpsertAsync(entry, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
            {
                return result;
            }
        }

        return VKResult.Success();
    }

    public Task<VKResult<VKMemoryEntry?>> GetByIdAsync(
        VKMemoryId id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_store.TryGetValue(id, out var entry))
        {
            var currentTenantId = _tenantCoordinate.TenantId;
            if (entry.TenantId != null && entry.TenantId != currentTenantId)
            {
                return Task.FromResult(VKResult.Success<VKMemoryEntry?>(null));
            }
            return Task.FromResult(VKResult.Success<VKMemoryEntry?>(entry));
        }

        return Task.FromResult(VKResult.Success<VKMemoryEntry?>(null));
    }

    public Task<VKResult<IReadOnlyList<VKMemoryEntry>>> GetByIdsAsync(
        IEnumerable<VKMemoryId> ids,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(ids);

        var currentTenantId = _tenantCoordinate.TenantId;
        var idSet = ids.ToHashSet();

        var result = _store.Values
            .Where(m => idSet.Contains(m.Id))
            .Where(m => m.TenantId is null || m.TenantId == currentTenantId)
            .ToList();

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKMemoryEntry>>(result));
    }

    public Task<VKResult<IReadOnlyList<VKMemoryEntry>>> QueryAsync(
        VKMemoryQuery query,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(query);

        var targetTenantId = query.TenantId ?? _tenantCoordinate.TenantId;

        var entries = _store.Values
            .Where(m => m.TenantId is null || m.TenantId == targetTenantId)
            .Where(m => !query.SessionId.HasValue || m.SessionId == query.SessionId.Value)
            .Where(m => !query.Category.HasValue || m.Category == query.Category.Value)
            .Where(m => MatchesScope(m.ExtendedScope, query.ExtendedScope))
            .OrderByDescending(m => m.CreatedAt)
            .Take(query.TopK)
            .ToList();

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKMemoryEntry>>(entries));
    }

    public Task<VKResult> DeleteAsync(
        VKMemoryId id,
        VKTenantId? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var targetTenant = tenantId ?? _tenantCoordinate?.TenantId;

        if (_store.TryGetValue(id, out var entry))
        {
            if (targetTenant is null || entry.TenantId is null || entry.TenantId == targetTenant)
            {
                _store.TryRemove(id, out _);
                _logger.MemoryEntryForgotten(id.ToString(), targetTenant?.Value.ToString());
            }
        }

        return Task.FromResult(VKResult.Success());
    }

    private static bool MatchesScope(IReadOnlyDictionary<string, string> entryScope, IReadOnlyDictionary<string, string> queryScope)
    {
        if (queryScope.Count == 0)
            return true;

        foreach (var (key, value) in queryScope)
        {
            if (!entryScope.TryGetValue(key, out var entryVal) || !string.Equals(entryVal, value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
