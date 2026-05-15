using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Defines the interface for loading and chunking documents.
/// </summary>
public interface IVKDocumentLoader
{
    /// <summary>
    /// Loads document chunks from a source.
    /// </summary>
    /// <param name="source">The document source (e.g., file path, URL).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of document chunks.</returns>
    Task<VKResult<IEnumerable<VKDocumentChunk>>> LoadAsync(string source, CancellationToken cancellationToken = default);
}
