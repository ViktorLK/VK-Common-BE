using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Retrieval.Internal;

internal sealed class DefaultAccessTracker : IVKAccessTracker
{
    private readonly IVKMemoryStore _store;
    private readonly TimeProvider _timeProvider;

    public DefaultAccessTracker(
        IVKMemoryStore store,
        TimeProvider timeProvider)
    {
        _store = VKGuard.NotNull(store);
        _timeProvider = VKGuard.NotNull(timeProvider);
    }

    public async Task<VKResult> RecordAccessAsync(VKMemoryId memoryId, CancellationToken cancellationToken = default)
    {
        return await RecordAccessBatchAsync([memoryId], cancellationToken).ConfigureAwait(false);
    }

    public async Task<VKResult> RecordAccessBatchAsync(IEnumerable<VKMemoryId> memoryIds, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(memoryIds);

        var idsList = new List<VKMemoryId>(memoryIds);
        if (idsList.Count == 0)
        {
            return VKResult.Success();
        }

        var getResult = await _store.GetByIdsAsync(idsList, cancellationToken).ConfigureAwait(false);
        if (getResult.IsFailure)
        {
            return VKResult.Failure(getResult.Errors);
        }

        var now = _timeProvider.GetUtcNow();
        var updatedEntries = new List<VKMemoryEntry>(getResult.Value.Count);

        foreach (var entry in getResult.Value)
        {
            updatedEntries.Add(entry with
            {
                AccessCount = entry.Category == VKMemoryCategory.LongTerm ? entry.AccessCount + 1 : entry.AccessCount,
                LastAccessedAt = now
            });
        }

        if (updatedEntries.Count > 0)
        {
            return await _store.UpsertBatchAsync(updatedEntries, cancellationToken).ConfigureAwait(false);
        }

        return VKResult.Success();
    }
}
