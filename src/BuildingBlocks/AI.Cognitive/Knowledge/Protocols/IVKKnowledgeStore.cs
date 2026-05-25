using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Knowledge: Manages Lorebooks and static facts to eliminate hallucinations.
/// Metaphor: Library - The external brain's knowledge repository.
/// Value: Project document retrieval (Industrial) and the World Setting Book (PWP).
/// </summary>
public interface IVKKnowledgeStore
{
    /// <summary>
    /// Retrieves relevant knowledge entries based on the provided context/input.
    /// </summary>
    Task<VKResult<IEnumerable<VKKnowledgeEntry>>> GetRelevantEntriesAsync(
        string personaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates a knowledge entry.
    /// </summary>
    Task<VKResult> UpsertEntryAsync(
        VKKnowledgeEntry entry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a knowledge entry by ID.
    /// </summary>
    Task<VKResult> DeleteEntryAsync(
        string entryId,
        CancellationToken cancellationToken = default);
}
