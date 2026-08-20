using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Knowledge: Manages Lorebooks and static facts to eliminate hallucinations.
/// Follows CS.01 and CS.03 patterns.
/// </summary>
public interface IVKKnowledgeStore
{
    /// <summary>
    /// Gets relevant knowledge entries for the specified knowledge IDs.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKKnowledgeEntry>>> GetKnowledgeEntriesAsync(
        IReadOnlyList<VKKnowledgeId> knowledgeIds,
        CancellationToken cancellationToken = default);
}
