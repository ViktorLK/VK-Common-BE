using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DataIngestion;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorIngest.Pipeline.Internal;

/// <summary>
/// Custom IngestionChunkWriter adapting Microsoft.Extensions.DataIngestion chunks
/// directly to VK.Blocks.VectorStore facade, preserving tenant isolation and VKResult semantics.
/// </summary>
internal sealed class VKVectorStoreWriter : IngestionChunkWriter<string>
{
    private readonly IVKIndexingService _indexingService;
    private readonly string _tenantId;
    private readonly string _documentId;
    private readonly string? _collectionName;
    private readonly string? _embeddingModelId;
    private readonly string? _modelVersion;
    private readonly IReadOnlyDictionary<string, object> _customMetadata;

    public VKVectorStoreWriter(
        IVKIndexingService indexingService,
        string tenantId,
        string documentId,
        string? collectionName,
        string? embeddingModelId,
        string? modelVersion,
        IReadOnlyDictionary<string, object>? customMetadata)
    {
        _indexingService = VKGuard.NotNull(indexingService);
        _tenantId = !string.IsNullOrWhiteSpace(tenantId) ? tenantId : "Default";
        _documentId = VKGuard.NotNullOrWhiteSpace(documentId);
        _collectionName = collectionName;
        _embeddingModelId = embeddingModelId;
        _modelVersion = modelVersion;
        _customMetadata = customMetadata ?? new Dictionary<string, object>();
    }

    public override async Task WriteAsync(
        IAsyncEnumerable<IngestionChunk<string>> chunks,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(chunks);

        var items = new List<(VKChunk Chunk, IReadOnlyDictionary<string, object> Metadata)>();
        var chunkIndex = 0;

        await foreach (var chunk in chunks.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var content = chunk.Content ?? string.Empty;
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var vkChunk = new VKChunk
            {
                Id = $"{_documentId}_{chunkIndex}",
                Content = content,
                ChunkIndex = chunkIndex++,
                StartOffset = 0,
                EndOffset = content.Length
            };

            var metadata = new Dictionary<string, object>(_customMetadata)
            {
                [VKIngestMetadataKeys.TenantId] = _tenantId,
                [VKIngestMetadataKeys.DocumentId] = _documentId,
                [VKIngestMetadataKeys.ChunkIndex] = vkChunk.ChunkIndex
            };

            if (_collectionName is not null)
            {
                metadata[VKIngestMetadataKeys.CollectionName] = _collectionName;
            }

            if (!string.IsNullOrWhiteSpace(_embeddingModelId))
            {
                metadata[VKIngestMetadataKeys.EmbeddingModelId] = _embeddingModelId;
            }

            if (!string.IsNullOrWhiteSpace(_modelVersion))
            {
                metadata[VKIngestMetadataKeys.ModelVersion] = _modelVersion;
            }

            items.Add((vkChunk, metadata));
        }

        if (items.Count > 0)
        {
            var indexResult = await _indexingService.IndexBatchAsync(items, cancellationToken).ConfigureAwait(false);
            if (indexResult.IsFailure)
            {
                throw new InvalidOperationException($"VectorStore indexing failed: {indexResult.Errors[0].Description}");
            }
        }
    }
}
