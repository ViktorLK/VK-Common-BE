using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// General interface for caching prompt completions.
/// </summary>
public interface IVKAICache
{
    /// <summary>
    /// Gets the cached completion for the specified prompt key.
    /// </summary>
    ValueTask<VKResult<string>> GetAsync(
        string promptKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Caches the completion response for a given prompt key.
    /// </summary>
    ValueTask<VKResult<bool>> SetAsync(
        string promptKey,
        string response,
        CancellationToken cancellationToken = default);
}
