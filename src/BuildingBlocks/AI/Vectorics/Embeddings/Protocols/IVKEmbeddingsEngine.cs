using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI;


/// <summary>
/// Defines the engine contract for generating embedding vectors.
/// In the VK AI ecosystem, "Engine" represents the core inference driver.
/// </summary>
public interface IVKEmbeddingsEngine
{
    /// <summary>
    /// Gets embeddings for the provided inputs.
    /// </summary>
    /// <param name="inputs">The list of text inputs to embed.</param>
    /// <param name="args">The optional execution arguments (e.g., ModelName override).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of embedding vectors.</returns>
    Task<VKResult<VKEmbeddingsResponse>> GetEmbeddingsAsync(
        IEnumerable<string> inputs,
        VKEmbeddingsArgs? args = null,
        CancellationToken cancellationToken = default);
}
