using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Defines the contract for orchestrating document ingesting into the corpus.
/// </summary>
public interface IVKCorpusIngestingService
{
    /// <summary>
    /// Ingests a document from the specified source path into the target collection with the associated knowledge lifecycle configuration.
    /// </summary>
    /// <param name="source">The source document path or URI.</param>
    /// <param name="collectionName">The target Vector Store collection name for tenant/domain isolation.</param>
    /// <param name="lifecycle">The knowledge lifecycle configuration options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<VKResult> IngestDocumentAsync(
        string source,
        string collectionName,
        VKKnowledgeLifecycle lifecycle,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a document and its associated vector chunks from the specified collection.
    /// </summary>
    /// <param name="documentId">The unique document ID.</param>
    /// <param name="collectionName">The collection name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<VKResult> DeleteDocumentAsync(
        string documentId,
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ingests a raw text block directly into the target collection under the specified document ID with the associated knowledge lifecycle configuration.
    /// </summary>
    /// <param name="rawText">The raw text content.</param>
    /// <param name="documentId">The unique document ID.</param>
    /// <param name="collectionName">The target Vector Store collection name.</param>
    /// <param name="lifecycle">The knowledge lifecycle configuration options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<VKResult> IngestTextAsync(
        string rawText,
        string documentId,
        string collectionName,
        VKKnowledgeLifecycle lifecycle,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queues an asynchronous document ingestion job with the associated knowledge lifecycle configuration and returns the generated Job ID.
    /// </summary>
    /// <param name="source">The source document path or URI.</param>
    /// <param name="collectionName">The target Vector Store collection name.</param>
    /// <param name="lifecycle">The knowledge lifecycle configuration options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the Job ID.</returns>
    Task<VKResult<string>> QueueIngestDocumentAsync(
        string source,
        string collectionName,
        VKKnowledgeLifecycle lifecycle,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of an ingestion job.
    /// </summary>
    /// <param name="jobId">The unique job ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the job status.</returns>
    Task<VKResult<VKIngestingJobStatus>> GetIngestingStatusAsync(
        string jobId,
        CancellationToken cancellationToken = default);
}
