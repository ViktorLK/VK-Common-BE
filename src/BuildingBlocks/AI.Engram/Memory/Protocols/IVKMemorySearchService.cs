using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Service interface for executing cognitive memory searches.
/// Combines vector search (payload copy + similarity score) with raw MemoryStore scope fallback.
/// </summary>
public interface IVKMemorySearchService
{
    /// <summary>
    /// Searches for memories matching semantic query criteria, returning memory entries along with similarity scores.
    /// </summary>
    Task<VKResult<IEnumerable<VKMemoryQueryResult>>> SearchAsync(
        VKMemoryQuery query,
        CancellationToken cancellationToken = default);
}
