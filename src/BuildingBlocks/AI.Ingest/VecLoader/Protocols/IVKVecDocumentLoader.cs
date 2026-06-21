using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest;

/// <summary>
/// Defines the interface for loading and chunking documents.
/// </summary>
public interface IVKVecDocumentLoader
{
    /// <summary>
    /// Loads document chunks from a source.
    /// </summary>
    /// <param name="source">The document source (e.g., file path, URL).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of document chunks.</returns>
    Task<VKResult<IEnumerable<VKVecDocumentChunk>>> LoadAsync(string source, CancellationToken cancellationToken = default);
}
