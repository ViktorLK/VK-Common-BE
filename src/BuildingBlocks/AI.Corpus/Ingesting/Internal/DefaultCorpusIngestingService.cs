using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Corpus.Diagnostics.Internal;
using VK.Blocks.Core;
using VK.Blocks.VectorIngest;
using VK.Blocks.VectorStore;

namespace VK.Blocks.AI.Corpus.Ingesting.Internal;

/// <summary>
/// Default implementation of <see cref="IVKCorpusIngestingService"/> delegating to <see cref="IVKIngestPipeline"/>.
/// </summary>
internal sealed class DefaultCorpusIngestingService : IVKCorpusIngestingService
{
    private readonly IVKIngestPipeline _ingestPipeline;
    private readonly IVKVectorStore _vectorStore;
    private readonly IVKIndexingService _indexingService;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly IVKIngestingStatusStore _statusStore;
    private readonly IVKCorpusPoisoningShield _poisoningShield;
    private readonly IVKKnowledgeHistoryStore _historyStore;
    private readonly ILogger<DefaultCorpusIngestingService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultCorpusIngestingService"/>.
    /// </summary>
    public DefaultCorpusIngestingService(
        IVKIngestPipeline ingestPipeline,
        IVKVectorStore vectorStore,
        IVKIndexingService indexingService,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        IVKJsonSerializer jsonSerializer,
        IVKIngestingStatusStore statusStore,
        IVKCorpusPoisoningShield poisoningShield,
        IVKKnowledgeHistoryStore historyStore,
        ILogger<DefaultCorpusIngestingService> logger)
    {
        _ingestPipeline = VKGuard.NotNull(ingestPipeline);
        _vectorStore = VKGuard.NotNull(vectorStore);
        _indexingService = VKGuard.NotNull(indexingService);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _statusStore = VKGuard.NotNull(statusStore);
        _poisoningShield = VKGuard.NotNull(poisoningShield);
        _historyStore = VKGuard.NotNull(historyStore);
        _logger = VKGuard.NotNull(logger);
    }

    /// <inheritdoc />
    public async Task<VKResult> IngestDocumentAsync(
        string source,
        string collectionName,
        VKKnowledgeLifecycle lifecycle,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(source);
        VKGuard.NotNullOrWhiteSpace(collectionName);
        VKGuard.NotNull(lifecycle);

        long startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
        var customMetadata = new Dictionary<string, object>
        {
            { "lifecycle", _jsonSerializer.Serialize(lifecycle) }
        };

        var result = await _ingestPipeline.ExecuteAsync(source, collectionName, customMetadata, cancellationToken).ConfigureAwait(false); // [CS.03]
        double durationMs = System.Diagnostics.Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
        CorpusDiagnostics.RecordIngestion(collectionName, result.IsSuccess, durationMs);
        return result;
    }

    /// <inheritdoc />
    public async Task<VKResult> DeleteDocumentAsync(
        string documentId,
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(documentId);
        VKGuard.NotNullOrWhiteSpace(collectionName);

        try
        {
            var collection = _vectorStore.Collection<object>(collectionName);
            return await collection.DeleteAsync(documentId, cancellationToken: cancellationToken).ConfigureAwait(false); // [CS.03]
        }
        catch (Exception ex)
        {
            CorpusLog.FailedToDeleteDocument(_logger, documentId, collectionName, ex);
            return VKResult.Failure(VKError.Failure("AI.Corpus.Delete.Failed", $"Delete failed: {ex.Message}")); // [CS.01]
        }
    }

    /// <inheritdoc />
    public async Task<VKResult> IngestTextAsync(
        string rawText,
        string documentId,
        string collectionName,
        VKKnowledgeLifecycle lifecycle,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(rawText);
        VKGuard.NotNullOrWhiteSpace(documentId);
        VKGuard.NotNullOrWhiteSpace(collectionName);
        VKGuard.NotNull(lifecycle);

        var shieldResult = await _poisoningShield.ValidateContentAsync(rawText, cancellationToken).ConfigureAwait(false);
        if (shieldResult.IsFailure)
        {
            return shieldResult;
        }

        try
        {
            var chunk = new VKChunk
            {
                Id = _guidGenerator.Create().ToString("N"), // [CS.06]
                Content = rawText,
                ChunkIndex = 0,
                StartOffset = 0,
                EndOffset = rawText.Length
            };

            var timestampUtc = _timeProvider.GetUtcNow(); // [CS.06]
            var existingVersionsResult = await _historyStore.GetVersionsAsync(documentId, cancellationToken).ConfigureAwait(false);
            int nextVersion = (existingVersionsResult.IsSuccess ? existingVersionsResult.Value.Count : 0) + 1;

            var versionSnapshot = new VKKnowledgeVersion
            {
                DocumentId = documentId,
                CollectionName = collectionName,
                Version = nextVersion,
                RawText = rawText,
                Lifecycle = lifecycle,
                CreatedAtUtc = timestampUtc
            };
            await _historyStore.SaveVersionAsync(versionSnapshot, cancellationToken).ConfigureAwait(false);

            var timestamp = timestampUtc.ToString("o");
            var metadata = new Dictionary<string, object>
            {
                { VKIngestMetadataKeys.DocumentId, documentId },
                { VKIngestMetadataKeys.ChunkIndex, 0 },
                { VKIngestMetadataKeys.TotalChunks, 1 },
                { VKIngestMetadataKeys.SourceUri, $"text://{documentId}" },
                { VKIngestMetadataKeys.IngestedAtUtc, timestamp },
                { VKIngestMetadataKeys.CollectionName, collectionName },
                { "lifecycle", _jsonSerializer.Serialize(lifecycle) },
                { "version", nextVersion }
            };

            return await _indexingService.IndexAsync(chunk, metadata, cancellationToken).ConfigureAwait(false); // [CS.03]
        }
        catch (Exception ex)
        {
            CorpusLog.FailedToIngestText(_logger, documentId, collectionName, ex);
            return VKResult.Failure(VKError.Failure("AI.Corpus.IngestText.Failed", $"Ingest text failed: {ex.Message}")); // [CS.01]
        }
    }

    /// <inheritdoc />
    public Task<VKResult<string>> QueueIngestDocumentAsync(
        string source,
        string collectionName,
        VKKnowledgeLifecycle lifecycle,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(source);
        VKGuard.NotNullOrWhiteSpace(collectionName);
        VKGuard.NotNull(lifecycle);

        var jobId = _guidGenerator.Create().ToString("N"); // [CS.06]
        _statusStore.UpdateStatus(jobId, VKIngestingStatus.Pending);

        var customMetadata = new Dictionary<string, object>
        {
            { "lifecycle", _jsonSerializer.Serialize(lifecycle) }
        };

        // Fire-and-forget background task executing the pipeline
        _ = Task.Run(async () =>
        {
            _statusStore.UpdateStatus(jobId, VKIngestingStatus.Processing);
            try
            {
                var result = await _ingestPipeline.ExecuteAsync(source, collectionName, customMetadata, CancellationToken.None).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    _statusStore.UpdateStatus(jobId, VKIngestingStatus.Completed);
                }
                else
                {
                    var firstErr = result.FirstError;
                    _statusStore.UpdateStatus(jobId, VKIngestingStatus.Failed, firstErr?.Description ?? "Unknown failure");
                }
            }
            catch (Exception ex)
            {
                CorpusLog.AsynchronousIngestionJobFailed(_logger, jobId, ex);
                _statusStore.UpdateStatus(jobId, VKIngestingStatus.Failed, ex.Message);
            }
        }, CancellationToken.None);

        return Task.FromResult(VKResult.Success(jobId));
    }

    /// <inheritdoc />
    public Task<VKResult<VKIngestingJobStatus>> GetIngestingStatusAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(jobId);

        var status = _statusStore.GetStatus(jobId);
        if (status is null)
        {
            return Task.FromResult(VKResult.Failure<VKIngestingJobStatus>(
                VKError.NotFound("AI.Corpus.Job.NotFound", $"Ingesting job with ID {jobId} was not found."))); // [CS.01]
        }

        return Task.FromResult(VKResult.Success(status));
    }

    /// <inheritdoc />
    public async Task<VKResult<IReadOnlyList<VKKnowledgeVersion>>> GetDocumentVersionsAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(documentId);
        return await _historyStore.GetVersionsAsync(documentId, cancellationToken).ConfigureAwait(false); // [CS.03]
    }

    /// <inheritdoc />
    public async Task<VKResult> RollbackDocumentVersionAsync(
        string documentId,
        string collectionName,
        int targetVersion,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(documentId);
        VKGuard.NotNullOrWhiteSpace(collectionName);

        var versionResult = await _historyStore.GetVersionAsync(documentId, targetVersion, cancellationToken).ConfigureAwait(false);
        if (versionResult.IsFailure)
        {
            return VKResult.Failure(versionResult.FirstError!); // [CS.01]
        }

        var targetSnapshot = versionResult.Value;

        // 1. Delete current document chunks from VectorStore
        var deleteResult = await DeleteDocumentAsync(documentId, collectionName, cancellationToken).ConfigureAwait(false);
        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }

        // 2. Re-ingest the target historical version text into VectorStore
        return await IngestTextAsync(targetSnapshot.RawText, documentId, collectionName, targetSnapshot.Lifecycle, cancellationToken).ConfigureAwait(false); // [CS.03]
    }
}
