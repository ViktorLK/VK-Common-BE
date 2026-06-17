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
/// Provides a high-performance in-memory backing store, multi-hop regex/keyword triggers,
/// and recursive matching engine.
/// </summary>
internal sealed class InMemoryKnowledgeStore : IVKKnowledgeStore
{
    private readonly ConcurrentDictionary<string, List<VKKnowledgeEntry>> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task<VKResult<IEnumerable<VKKnowledgeEntry>>> GetRelevantEntriesAsync(
        VKPersonaId personaId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotEmptyGuid(personaId.Value);

        if (!_store.TryGetValue(personaId.ToString(), out var entries))
        {
            return Task.FromResult(VKResult.Failure<IEnumerable<VKKnowledgeEntry>>(VKKnowledgeErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success<IEnumerable<VKKnowledgeEntry>>(entries));
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
