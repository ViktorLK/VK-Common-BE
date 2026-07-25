using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation;

public interface IVKConsolidationPersistenceManager
{
    /// <summary>
    /// Persists a list of memory entries with retry + DLQ fallback + vector indexing.
    /// </summary>
    Task<VKResult> PersistEntriesAsync(
        IReadOnlyList<VKMemoryEntry> entries,
        CancellationToken cancellationToken = default);
}
