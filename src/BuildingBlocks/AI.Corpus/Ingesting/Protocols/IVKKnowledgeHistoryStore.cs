using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Contract for persisting and retrieving historical version snapshots of corpus knowledge documents.
/// </summary>
public interface IVKKnowledgeHistoryStore
{
    /// <summary>
    /// Saves a new version snapshot into the history store.
    /// </summary>
    /// <param name="version">The version snapshot to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<VKResult> SaveVersionAsync(VKKnowledgeVersion version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all historical versions for a document.
    /// </summary>
    /// <param name="documentId">The unique document ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the list of versions ordered by version number.</returns>
    Task<VKResult<IReadOnlyList<VKKnowledgeVersion>>> GetVersionsAsync(string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific historical version of a document.
    /// </summary>
    /// <param name="documentId">The unique document ID.</param>
    /// <param name="version">The target version number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the target version snapshot.</returns>
    Task<VKResult<VKKnowledgeVersion>> GetVersionAsync(string documentId, int version, CancellationToken cancellationToken = default);
}
