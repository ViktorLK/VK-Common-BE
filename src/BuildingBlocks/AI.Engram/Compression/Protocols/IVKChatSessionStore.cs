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

    /// <summary>
    /// Updates the session memory fields.
    /// </summary>
    Task<VKResult> UpdateSessionMemoryAsync(
        VKChatSessionId id,
        string summary,
        string? narrativeSummary = null,
        string? structuredFacts = null,
        string? relationGraph = null,
        string? timeline = null,
        string? contradictions = null,
        string? actionItems = null,
        string? confidenceAnnotations = null,
        string? predictiveCues = null,
        float? valence = null,
        float? arousal = null,
        CancellationToken cancellationToken = default) => UpdateSummaryAsync(id, summary, cancellationToken);
}
