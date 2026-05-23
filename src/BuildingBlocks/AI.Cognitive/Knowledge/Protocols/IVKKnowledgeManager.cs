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
public interface IVKKnowledgeManager
{
    /// <summary>
    /// Retrieves relevant knowledge entries based on the provided context/input.
    /// </summary>
    Task<VKResult<IEnumerable<VKKnowledgeEntry>>> GetRelevantEntriesAsync(
        string context,
        string? themeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all knowledge entries for a specific theme or globally.
    /// </summary>
    Task<VKResult<IEnumerable<VKKnowledgeEntry>>> GetAllEntriesAsync(
        string? themeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates a knowledge entry.
    /// </summary>
    Task<VKResult> UpsertEntryAsync(
        VKKnowledgeEntry entry,
        string? themeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a knowledge entry by ID.
    /// </summary>
    Task<VKResult> DeleteEntryAsync(
        string entryId,
        string? themeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records the triggered knowledge entries in a session, setting their cooldown and sticky turns.
    /// </summary>
    Task<VKResult> RecordTriggersAsync(
        string sessionId,
        IEnumerable<string> triggeredEntryIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Advances the conversation turn for a session, decrementing remaining cooldown and sticky turns.
    /// </summary>
    Task<VKResult> AdvanceSessionTurnAsync(
        string sessionId,
        CancellationToken cancellationToken = default);
}
