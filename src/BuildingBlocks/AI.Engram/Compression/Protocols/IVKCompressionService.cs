using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Provides services for compressing and summarizing AI chat sessions.
/// </summary>
public interface IVKCompressionService
{
    /// <summary>
    /// Processes compression for a specific chat session if it exceeds token or turn limits.
    /// </summary>
    /// <param name="sessionId">The chat session ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the updated summary if compression occurred; otherwise, a success result with null.</returns>
    Task<VKResult<string?>> CompressSessionAsync(VKChatSessionId sessionId, CancellationToken cancellationToken = default);
}
