using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Defines the pure contract for generating mathematical vector representations from text.
/// Contains no high-level AI context or orchestration semantics.
/// </summary>
public interface IVKEmbeddingsEngine
{
    /// <summary>
    /// Generates a vector representation for the specified input text.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resulting high-dimensional vector.</returns>
    Task<VKResult<VKVector>> GenerateAsync(string text, CancellationToken cancellationToken = default);
}
