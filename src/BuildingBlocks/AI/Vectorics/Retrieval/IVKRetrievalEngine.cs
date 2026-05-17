using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for an AI-driven retrieval engine.
/// </summary>
public interface IVKRetrievalEngine
{
    /// <summary>
    /// Performs a semantic search for the specified query.
    /// </summary>
    Task<VKResult<IEnumerable<string>>> SearchAsync(
        string query,
        IVKAIArgs? args = null,
        CancellationToken cancellationToken = default);
}
