using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Defines the contract for a semantic cache service using vector store and embedding engines.
/// </summary>
public interface IVKSemanticCacheService
{
    /// <summary>
    /// Attempts to retrieve a cached value by querying similarity of the prompt text.
    /// </summary>
    Task<VKResult<string?>> GetAsync(string prompt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Caches a response text associated with a prompt text.
    /// </summary>
    Task<VKResult> SetAsync(string prompt, string response, CancellationToken cancellationToken = default);
}
