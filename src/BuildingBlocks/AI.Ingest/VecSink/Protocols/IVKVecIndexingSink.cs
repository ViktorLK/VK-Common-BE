using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest;

/// <summary>
/// Defines the interface for writing document chunks and embeddings to a vector indexing sink.
/// </summary>
public interface IVKVecIndexingSink
{
    /// <summary>
    /// Writes chunks and their corresponding embeddings to the sink.
    /// </summary>
    /// <param name="chunks">The document chunks.</param>
    /// <param name="embeddings">The embeddings vectors.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<VKResult> WriteAsync(
        IEnumerable<VKVecDocumentChunk> chunks,
        IEnumerable<VKEmbeddingsVector> embeddings,
        CancellationToken cancellationToken = default);
}
