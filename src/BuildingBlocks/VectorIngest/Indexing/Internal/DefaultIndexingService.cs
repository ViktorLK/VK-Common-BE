using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.VectorIngest.Common.Models.Internal;
using VK.Blocks.VectorStore;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Indexing.Internal; // [AP.03] Internal namespace

/// <summary>
/// Default implementation of <see cref="IVKIndexingService"/>.
/// </summary>
internal sealed class DefaultIndexingService : IVKIndexingService // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly IVKEmbeddingsEngine _embeddingsEngine;
    private readonly IVKVectorStore _vectorStore;
    private readonly VKVectorStoreDefaultsOptions _defaults;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultIndexingService"/>.
    /// </summary>
    public DefaultIndexingService(
        IVKEmbeddingsEngine embeddingsEngine,
        IVKVectorStore vectorStore,
        IOptions<VKVectorStoreDefaultsOptions> defaultsOptions)
    {
        _embeddingsEngine = VKGuard.NotNull(embeddingsEngine); // [AP.01] VKGuard boundary
        _vectorStore = VKGuard.NotNull(vectorStore);
        _defaults = defaultsOptions?.Value ?? new VKVectorStoreDefaultsOptions();
    }

    /// <inheritdoc />
    public async Task<VKResult> IndexAsync(
        VKChunk chunk,
        IReadOnlyDictionary<string, object> metadata,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(chunk); // [AP.01] VKGuard boundary
        VKGuard.NotNull(metadata);

        try
        {
            var embedResult = await _embeddingsEngine.GenerateAsync(chunk.Content, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
            if (embedResult.IsFailure)
            {
                return embedResult; // [CS.01] Result only
            }

            var tenantId = metadata.TryGetValue(VKIngestMetadataKeys.DocumentId, out var docIdObj) ? docIdObj.ToString() ?? "Default" : "Default";

            var properties = metadata.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? string.Empty);
            var vectorMetadata = new VKVectorMetadata
            {
                TenantId = tenantId,
                Source = metadata.TryGetValue(VKIngestMetadataKeys.SourceUri, out var sourceUriObj) ? sourceUriObj.ToString() : null,
                Properties = properties
            };

            var document = new VectorStoreDocument(chunk.Content, vectorMetadata);
            var collectionName = metadata.TryGetValue(VKIngestMetadataKeys.CollectionName, out var colNameObj) && colNameObj is not null
                ? colNameObj.ToString() ?? _defaults.DefaultCollection
                : _defaults.DefaultCollection;
            var collection = _vectorStore.Collection<VectorStoreDocument>(collectionName);
            var id = $"{tenantId}_{chunk.ChunkIndex}";

            return await collection.UpsertAsync(id, document, embedResult.Value, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
        }
        catch (Exception ex)
        {
            return VKResult.Failure(VKError.Failure(
                "AI.Ingest.Indexing.Failed",
                $"Indexing failed: {ex.Message}")); // [CS.01] Map exceptions
        }
    }

    /// <inheritdoc />
    public async Task<VKResult> IndexBatchAsync(
        IReadOnlyList<(VKChunk Chunk, IReadOnlyDictionary<string, object> Metadata)> items,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(items); // [AP.01] VKGuard boundary

        if (items.Count == 0)
            return VKResult.Success();

        try
        {
            var records = new List<(string Id, VectorStoreDocument Document, VKVector Vector)>();

            foreach (var item in items)
            {
                var embedResult = await _embeddingsEngine.GenerateAsync(item.Chunk.Content, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
                if (embedResult.IsFailure)
                {
                    return embedResult; // [CS.01] Result only
                }

                var tenantId = item.Metadata.TryGetValue(VKIngestMetadataKeys.DocumentId, out var docIdObj) ? docIdObj.ToString() ?? "Default" : "Default";
                var properties = item.Metadata.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? string.Empty);
                var vectorMetadata = new VKVectorMetadata
                {
                    TenantId = tenantId,
                    Source = item.Metadata.TryGetValue(VKIngestMetadataKeys.SourceUri, out var sourceUriObj) ? sourceUriObj.ToString() : null,
                    Properties = properties
                };

                var document = new VectorStoreDocument(item.Chunk.Content, vectorMetadata);
                var id = $"{tenantId}_{item.Chunk.ChunkIndex}";

                records.Add((id, document, embedResult.Value));
            }

            var firstItemMetadata = items[0].Metadata;
            var collectionName = firstItemMetadata.TryGetValue(VKIngestMetadataKeys.CollectionName, out var colNameObj) && colNameObj is not null
                ? colNameObj.ToString() ?? _defaults.DefaultCollection
                : _defaults.DefaultCollection;

            // [Capability degradation check]
            if (_vectorStore is IVKBulkCapableVectorStore bulkStore)
            {
                return await bulkStore.UpsertBatchAsync(collectionName, records, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
            }

            var collection = _vectorStore.Collection<VectorStoreDocument>(collectionName);
            return await collection.UpsertBatchAsync(records, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
        }
        catch (Exception ex)
        {
            return VKResult.Failure(VKError.Failure(
                "AI.Ingest.Indexing.BatchFailed",
                $"Batch indexing failed: {ex.Message}")); // [CS.01] Map exceptions
        }
    }
}
