using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Knowledge.Internal;

/// <summary>
/// Basic concrete implementation of <see cref="IVKKnowledgeStore"/>.
/// </summary>
internal sealed class InMemoryKnowledgeStore : IVKKnowledgeStore
{
    private readonly ConcurrentDictionary<string, List<VKKnowledgeEntry>> _store = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryKnowledgeStore()
    {
    }

    public Task<VKResult<IReadOnlyList<VKKnowledgeEntry>>> GetKnowledgeEntriesAsync(
        IReadOnlyList<VKKnowledgeId> knowledgeIds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var resultList = new List<VKKnowledgeEntry>();

        if (knowledgeIds.Count == 0)
        {
            foreach (var list in _store.Values)
            {
                resultList.AddRange(list);
            }
            return Task.FromResult(VKResult.Success<IReadOnlyList<VKKnowledgeEntry>>(resultList));
        }

        foreach (var kId in knowledgeIds)
        {
            if (_store.TryGetValue(kId.ToString(), out var entries))
            {
                resultList.AddRange(entries);
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKKnowledgeEntry>>(resultList));
    }

    public InMemoryKnowledgeStore Seed(VKKnowledgeEntry knowledgeEntry)
    {
        VKGuard.NotNull(knowledgeEntry);
        var list = _store.GetOrAdd(knowledgeEntry.Id.ToString(), _ => []);
        list.Add(knowledgeEntry);

        return this;
    }

    public InMemoryKnowledgeStore Seed(IEnumerable<VKKnowledgeEntry> knowledgeEntries)
    {
        VKGuard.NotNull(knowledgeEntries);
        foreach (var groupKnowledge in knowledgeEntries.GroupBy(x => x.Id))
        {
            var list = _store.GetOrAdd(groupKnowledge.Key.ToString(), _ => []);
            list.AddRange([.. groupKnowledge]);
        }

        return this;
    }

    public InMemoryKnowledgeStore Remove(VKKnowledgeId knowledgeEntryId)
    {
        _store.TryRemove(knowledgeEntryId.ToString(), out _);

        return this;
    }

    public InMemoryKnowledgeStore Clear()
    {
        _store.Clear();

        return this;
    }
}
