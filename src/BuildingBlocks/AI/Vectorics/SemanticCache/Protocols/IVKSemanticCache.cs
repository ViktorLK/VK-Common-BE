using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for a Semantic Cache.
/// Stores and retrieves AI responses based on semantic similarity of prompts.
/// </summary>
public interface IVKSemanticCache
{
    /// <summary>
    /// Tries to get a cached response for a given prompt.
    /// </summary>
    /// <param name="prompt">The input prompt.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached response if found, otherwise failure.</returns>
    Task<VKResult<string>> GetAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a response in the cache for a given prompt.
    /// </summary>
    /// <param name="prompt">The input prompt.</param>
    /// <param name="response">The AI response to cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A success result.</returns>
    Task<VKResult> SetAsync(string prompt, string response, CancellationToken cancellationToken = default);
}
