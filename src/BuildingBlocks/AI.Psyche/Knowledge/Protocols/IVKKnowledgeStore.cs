using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Knowledge: Manages Lorebooks and static facts to eliminate hallucinations.
/// Metaphor: Library - The external brain's knowledge repository.
/// Value: Project document retrieval (Industrial) and the World Setting Book (PWP).
/// </summary>
public interface IVKKnowledgeStore
{
    Task<VKResult<IEnumerable<VKKnowledgeEntry>>> GetRelevantEntriesAsync(
        string personaId,
        CancellationToken cancellationToken = default);
}
