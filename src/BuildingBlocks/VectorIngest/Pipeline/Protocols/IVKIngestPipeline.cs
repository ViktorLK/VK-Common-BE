using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest;

/// <summary>
/// Defines the contract for orchestrating the document ingestion and indexing pipeline.
/// </summary>
public interface IVKIngestPipeline
{
    /// <summary>
    /// Executes the ingestion pipeline for a given document source.
    /// Loads/chunks the document, generates embeddings, and writes them to the indexing sink.
    /// </summary>
    /// <param name="source">The document source (e.g., path or content identifier).</param>
    /// <param name="collectionName">The target Vector Store collection name (optional).</param>
    /// <param name="customMetadata">Additional metadata to attach to the ingested chunks (optional).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure of the pipeline execution.</returns>
    Task<VKResult> ExecuteAsync(
        string source,
        string collectionName = "",
        IReadOnlyDictionary<string, object>? customMetadata = null,
        CancellationToken cancellationToken = default);
}
