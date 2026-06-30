using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Interface for semantic caching of prompt completions based on embedding similarity.
/// </summary>
public interface IVKAISemanticCache
{
    /// <summary>
    /// Checks for a cached response whose prompt is semantically similar to the target prompt.
    /// </summary>
    ValueTask<VKResult<string>> GetSimilarAsync(
        string prompt,
        double similarityThreshold,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the response in semantic cache mapped to the prompt.
    /// </summary>
    ValueTask<VKResult<bool>> SetSemanticAsync(
        string prompt,
        string response,
        CancellationToken cancellationToken = default);
}
