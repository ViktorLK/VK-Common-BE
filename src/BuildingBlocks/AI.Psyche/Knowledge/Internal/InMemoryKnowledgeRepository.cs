using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IVKPsycheKnowledgeRepository"/>.
/// Follows AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class InMemoryKnowledgeRepository : IVKPsycheKnowledgeRepository
{
    private readonly ConcurrentDictionary<VKKnowledgeId, VKKnowledgeEntry> _store = new();

    public InMemoryKnowledgeRepository()
    {
    }

    public Task<VKResult<VKKnowledgeEntry>> FindByIdAsync(VKKnowledgeId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (id.IsEmpty)
        {
            return Task.FromResult(VKResult.Failure<VKKnowledgeEntry>(VKKnowledgeErrors.NotFound));
        }

        if (_store.TryGetValue(id, out var entry))
        {
            return Task.FromResult(VKResult.Success(entry));
        }

        return Task.FromResult(VKResult.Failure<VKKnowledgeEntry>(VKKnowledgeErrors.NotFound));
    }

    public Task<VKResult<IReadOnlyList<VKKnowledgeEntry>>> ListByIdsAsync(
        IReadOnlyList<VKKnowledgeId> ids,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(ids);
        ct.ThrowIfCancellationRequested();

        var list = new List<VKKnowledgeEntry>(ids.Count);
        foreach (var id in ids)
        {
            if (_store.TryGetValue(id, out var entry))
            {
                list.Add(entry);
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKKnowledgeEntry>>(list));
    }

    public Task<VKResult<IReadOnlyList<VKKnowledgeEntry>>> ListAllAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<VKKnowledgeEntry> list = [.. _store.Values];
        return Task.FromResult(VKResult.Success(list));
    }

    public Task<bool> ExistsAsync(VKKnowledgeId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(!id.IsEmpty && _store.ContainsKey(id));
    }

    public Task<VKResult> AddAsync(VKKnowledgeEntry item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!_store.TryAdd(item.Id, item))
        {
            return Task.FromResult(VKResult.Failure(VKKnowledgeErrors.AlreadyExists));
        }

        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> UpdateAsync(VKKnowledgeEntry item, CancellationToken ct = default)
    {
        VKGuard.NotNull(item);
        ct.ThrowIfCancellationRequested();

        if (!_store.ContainsKey(item.Id))
        {
            return Task.FromResult(VKResult.Failure(VKKnowledgeErrors.NotFound));
        }

        _store[item.Id] = item;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult> DeleteAsync(VKKnowledgeId id, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (id.IsEmpty || !_store.TryRemove(id, out _))
        {
            return Task.FromResult(VKResult.Failure(VKKnowledgeErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success());
    }


    public InMemoryKnowledgeRepository Seed(VKKnowledgeEntry knowledgeEntry)
    {
        VKGuard.NotNull(knowledgeEntry);
        _store[knowledgeEntry.Id] = knowledgeEntry;
        return this;
    }

    public InMemoryKnowledgeRepository Seed(IEnumerable<VKKnowledgeEntry> knowledgeEntries)
    {
        VKGuard.NotNull(knowledgeEntries);
        foreach (var entry in knowledgeEntries)
        {
            _store[entry.Id] = entry;
        }
        return this;
    }

    public InMemoryKnowledgeRepository Remove(VKKnowledgeId knowledgeEntryId)
    {
        _store.TryRemove(knowledgeEntryId, out _);
        return this;
    }

    public InMemoryKnowledgeRepository Clear()
    {
        _store.Clear();
        return this;
    }
}
