using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorIngest.Indexing.Internal; // [AP.03] Internal namespace

/// <summary>
/// Default implementation of <see cref="IVKIndexingService"/>.
/// Generates deterministic IDs based on Hash(TenantId + DocumentId + ContentHash) [AP.01, CS.01, CS.03].
/// </summary>
internal sealed class DefaultIndexingService : IVKIndexingService // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly IVKEmbeddingsEngine _embeddingsEngine;
    private readonly IVKVectorStore _vectorStore;
    private readonly VKVectorStoreOptions _defaults;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultIndexingService"/>.
    /// </summary>
    public DefaultIndexingService(
        IVKEmbeddingsEngine embeddingsEngine,
        IVKVectorStore vectorStore,
        IOptions<VKVectorStoreOptions> defaultsOptions)
    {
        _embeddingsEngine = VKGuard.NotNull(embeddingsEngine); // [AP.01] VKGuard boundary
        _vectorStore = VKGuard.NotNull(vectorStore);
        _defaults = defaultsOptions?.Value ?? new VKVectorStoreOptions();
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

            var tenantId = metadata.TryGetValue(VKIngestMetadataKeys.TenantId, out var tenantObj) && !string.IsNullOrWhiteSpace(tenantObj?.ToString())
                ? tenantObj.ToString()!
                : "Default";

            var documentId = metadata.TryGetValue(VKIngestMetadataKeys.DocumentId, out var docIdObj) && !string.IsNullOrWhiteSpace(docIdObj?.ToString())
                ? docIdObj.ToString()!
                : "Doc";

            var contentHash = metadata.TryGetValue(VKIngestMetadataKeys.ContentHash, out var hashObj) && !string.IsNullOrWhiteSpace(hashObj?.ToString())
                ? hashObj.ToString()!
                : ComputeContentHash(chunk.Content);

            var deterministicId = ComputeDeterministicChunkId(tenantId, documentId, contentHash);

            var properties = new Dictionary<string, string>();
            foreach (var kvp in metadata)
            {
                properties[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
            }

            var vectorMetadata = new VKVectorMetadata
            {
                TenantId = tenantId,
                Source = metadata.TryGetValue(VKIngestMetadataKeys.SourceUri, out var sourceUriObj) ? sourceUriObj?.ToString() : null,
                Properties = properties
            };

            var document = new VKVectorDocument
            {
                Content = chunk.Content,
                Metadata = vectorMetadata
            };

            var collectionName = metadata.TryGetValue(VKIngestMetadataKeys.CollectionName, out var colNameObj) && colNameObj is not null
                ? colNameObj.ToString() ?? _defaults.DefaultCollection
                : _defaults.DefaultCollection;

            var collection = _vectorStore.Collection<VKVectorDocument>(collectionName);

            return await collection.UpsertAsync(deterministicId, document, embedResult.Value, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
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
        {
            return VKResult.Success();
        }

        try
        {
            var records = new List<(string Id, VKVectorDocument Document, VKVector Vector)>(items.Count);

            foreach (var item in items)
            {
                var embedResult = await _embeddingsEngine.GenerateAsync(item.Chunk.Content, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
                if (embedResult.IsFailure)
                {
                    return embedResult; // [CS.01] Result only
                }

                var tenantId = item.Metadata.TryGetValue(VKIngestMetadataKeys.TenantId, out var tenantObj) && !string.IsNullOrWhiteSpace(tenantObj?.ToString())
                    ? tenantObj.ToString()!
                    : "Default";

                var documentId = item.Metadata.TryGetValue(VKIngestMetadataKeys.DocumentId, out var docIdObj) && !string.IsNullOrWhiteSpace(docIdObj?.ToString())
                    ? docIdObj.ToString()!
                    : "Doc";

                var contentHash = item.Metadata.TryGetValue(VKIngestMetadataKeys.ContentHash, out var hashObj) && !string.IsNullOrWhiteSpace(hashObj?.ToString())
                    ? hashObj.ToString()!
                    : ComputeContentHash(item.Chunk.Content);

                var deterministicId = ComputeDeterministicChunkId(tenantId, documentId, contentHash);

                var properties = new Dictionary<string, string>();
                foreach (var kvp in item.Metadata)
                {
                    properties[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                }

                var vectorMetadata = new VKVectorMetadata
                {
                    TenantId = tenantId,
                    Source = item.Metadata.TryGetValue(VKIngestMetadataKeys.SourceUri, out var sourceUriObj) ? sourceUriObj?.ToString() : null,
                    Properties = properties
                };

                var document = new VKVectorDocument
                {
                    Content = item.Chunk.Content,
                    Metadata = vectorMetadata
                };

                records.Add((deterministicId, document, embedResult.Value));
            }

            var firstItemMetadata = items[0].Metadata;
            var collectionName = firstItemMetadata.TryGetValue(VKIngestMetadataKeys.CollectionName, out var colNameObj) && colNameObj is not null
                ? colNameObj.ToString() ?? _defaults.DefaultCollection
                : _defaults.DefaultCollection;

            var collection = _vectorStore.Collection<VKVectorDocument>(collectionName);
            return await collection.UpsertBatchAsync(records, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
        }
        catch (Exception ex)
        {
            return VKResult.Failure(VKError.Failure(
                "AI.Ingest.Indexing.BatchFailed",
                $"Batch indexing failed: {ex.Message}")); // [CS.01] Map exceptions
        }
    }

    private static string ComputeDeterministicChunkId(string tenantId, string documentId, string contentHash)
    {
        var rawKey = $"{tenantId}:{documentId}:{contentHash}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static string ComputeContentHash(string content)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
