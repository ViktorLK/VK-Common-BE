using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs.Outbox.Internal;

internal sealed class DefaultJobOutbox : IVKJobOutbox
{
    private readonly ConcurrentDictionary<string, VKJobOutboxEntry> _entries = new();

    public Task<VKResult> SaveAsync(VKJobOutboxEntry entry, CancellationToken ct = default)
    {
        VKGuard.NotNull(entry);
        _entries[entry.Id] = entry;
        return Task.FromResult(VKResult.Success());
    }

    public Task<VKResult<IReadOnlyList<VKJobOutboxEntry>>> GetUnprocessedAsync(int batchSize = 100, CancellationToken ct = default)
    {
        IReadOnlyList<VKJobOutboxEntry> unprocessed = _entries.Values
            .Where(e => !e.IsProcessed)
            .Take(batchSize)
            .ToList();

        return Task.FromResult(VKResult.Success(unprocessed));
    }

    public Task<VKResult> MarkProcessedAsync(string id, CancellationToken ct = default)
    {
        VKGuard.NotNullOrWhiteSpace(id);
        if (_entries.TryGetValue(id, out var existing))
        {
            _entries[id] = existing with { ProcessedAt = System.DateTimeOffset.UtcNow };
        }

        return Task.FromResult(VKResult.Success());
    }
}
