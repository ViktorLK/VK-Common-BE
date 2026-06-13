using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus.KnowledgeSourcing.Internal;

/// <summary>
/// In-memory implementation of <see cref="IVKStaticKnowledgeLifecycleStore"/> for testing or basic scenarios.
/// Offers thread-safe in-memory backing storage.
/// Follows BB.01 / AP.03.
/// </summary>
internal sealed class InMemoryKnowledgeLifecycleStore : IVKStaticKnowledgeLifecycleStore
{
    private readonly ConcurrentDictionary<VKKnowledgeId, VKKnowledgeLifecycleEntry> _entries = new();

    /// <summary>
    /// Initializes a new instance of <see cref="InMemoryKnowledgeLifecycleStore"/>.
    /// </summary>
    public InMemoryKnowledgeLifecycleStore()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InMemoryKnowledgeLifecycleStore"/> with initial entries.
    /// </summary>
    public InMemoryKnowledgeLifecycleStore(IEnumerable<VKKnowledgeLifecycleEntry> entries)
    {
        Seed(entries);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<VKKnowledgeId, VKKnowledgeLifecycleEntry> GetLifecycleEntries(IEnumerable<VKKnowledgeId> ids)
    {
        VKGuard.NotNull(ids);
        var result = new Dictionary<VKKnowledgeId, VKKnowledgeLifecycleEntry>();
        foreach (var id in ids)
        {
            if (_entries.TryGetValue(id, out var entry))
            {
                result[id] = entry;
            }
        }
        return result;
    }

    /// <summary>
    /// Seeds a single knowledge lifecycle entry into the store.
    /// </summary>
    public InMemoryKnowledgeLifecycleStore Seed(VKKnowledgeLifecycleEntry entry)
    {
        VKGuard.NotNull(entry);
        _entries[entry.Knowledge.Id] = entry;
        return this;
    }

    /// <summary>
    /// Seeds a collection of knowledge lifecycle entries into the store.
    /// </summary>
    public InMemoryKnowledgeLifecycleStore Seed(IEnumerable<VKKnowledgeLifecycleEntry> entries)
    {
        VKGuard.NotNull(entries);
        foreach (var entry in entries)
        {
            _entries[entry.Knowledge.Id] = entry;
        }
        return this;
    }

    /// <summary>
    /// Removes a knowledge lifecycle entry from the store.
    /// </summary>
    public InMemoryKnowledgeLifecycleStore Remove(VKKnowledgeId id)
    {
        _entries.TryRemove(id, out _);
        return this;
    }

    /// <summary>
    /// Clears all entries from the store.
    /// </summary>
    public InMemoryKnowledgeLifecycleStore Clear()
    {
        _entries.Clear();
        return this;
    }
}
