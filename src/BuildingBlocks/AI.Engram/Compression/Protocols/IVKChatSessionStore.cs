using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression;

/// <summary>
/// Defines a store for managing chat sessions and their summaries.
/// </summary>
public interface IVKChatSessionStore
{
    /// <summary>
    /// Gets an existing session by ID.
    /// </summary>
    Task<VKResult<VKChatSession>> GetAsync(
        VKChatSessionId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the summary of the specified session.
    /// </summary>
    Task<VKResult> UpdateSummaryAsync(
        VKChatSessionId id,
        string summary,
        CancellationToken cancellationToken = default);
}
