using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.BackgroundJobs;

/// <summary>
/// Contract for transactional Outbox pattern job enqueueing.
/// </summary>
public interface IVKJobOutbox
{
    Task<VKResult> SaveAsync(VKJobOutboxEntry entry, CancellationToken ct = default);
    Task<VKResult<IReadOnlyList<VKJobOutboxEntry>>> GetUnprocessedAsync(int batchSize = 100, CancellationToken ct = default);
    Task<VKResult> MarkProcessedAsync(string id, CancellationToken ct = default);
}
