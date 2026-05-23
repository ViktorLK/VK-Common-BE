using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Hook interface for external vector store providers to resolve semantic knowledge matches.
/// Under AP.03 (Public API Surface), this interface is public and has the VK prefix.
/// </summary>
public interface IVKSemanticKnowledgeProvider
{
    /// <summary>
    /// Performs semantic similarity search against a vector database based on the context.
    /// Under CS.01, returns a <see cref="VKResult{T}"/> containing triggered entries.
    /// </summary>
    Task<VKResult<IEnumerable<VKKnowledgeEntry>>> SearchSemanticAsync(
        string context,
        string? themeId = null,
        CancellationToken cancellationToken = default);
}
