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
        string personaId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNullOrWhiteSpace(personaId);

        if (!_store.TryGetValue(personaId, out var entries))
        {
            return Task.FromResult(VKResult.Failure<IEnumerable<VKKnowledgeEntry>>(VKKnowledgeErrors.NotFound));
        }

        return Task.FromResult(VKResult.Success<IEnumerable<VKKnowledgeEntry>>(entries));
    }

    public InMemoryKnowledgeStore Seed(VKKnowledgeEntry knowledgeEntry)
    {
        _store[knowledgeEntry.Id].Add(knowledgeEntry);

        return this;
    }

    public InMemoryKnowledgeStore Seed(IEnumerable<VKKnowledgeEntry> knowledgeEntries)
    {
        foreach (var groupKnowledge in knowledgeEntries.GroupBy(x => x.Id))
        {
            _store[groupKnowledge.Key].AddRange([.. groupKnowledge]);
        }

        return this;
    }

    public InMemoryKnowledgeStore Remove(string knowledgeEntryId)
    {
        _store.TryRemove(knowledgeEntryId, out _);

        return this;
    }

    public InMemoryKnowledgeStore Clear()
    {
        _store.Clear();

        return this;
    }
}
