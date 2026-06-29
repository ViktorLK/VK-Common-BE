using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Enrichment.Internal; // [AP.03] Internal namespace

/// <summary>
/// Default implementation of <see cref="IVKChunkMetadataEnricher"/>.
/// </summary>
internal sealed class DefaultChunkMetadataEnricher : IVKChunkMetadataEnricher // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultChunkMetadataEnricher"/>.
    /// </summary>
    public DefaultChunkMetadataEnricher(TimeProvider timeProvider)
    {
        _timeProvider = VKGuard.NotNull(timeProvider); // [AP.01] VKGuard boundary
    }

    /// <inheritdoc />
    public VKResult<IReadOnlyDictionary<string, object>> Enrich(VKChunk chunk, VKEnrichmentContext context)
    {
        VKGuard.NotNull(chunk); // [AP.01] VKGuard boundary
        VKGuard.NotNull(context);

        try
        {
            var contentBytes = Encoding.UTF8.GetBytes(chunk.Content);
            var hashBytes = SHA256.HashData(contentBytes);
            var contentHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

            var timestamp = _timeProvider.GetUtcNow().ToString("o"); // [CS.06] Use TimeProvider

            var enriched = new Dictionary<string, object>
            {
                { VKIngestMetadataKeys.DocumentId, context.DocumentId },
                { VKIngestMetadataKeys.ChunkIndex, chunk.ChunkIndex },
                { VKIngestMetadataKeys.TotalChunks, context.TotalChunks },
                { VKIngestMetadataKeys.ContentHash, contentHash },
                { VKIngestMetadataKeys.DocumentHash, context.DocumentHash },
                { VKIngestMetadataKeys.SourceUri, context.SourceUri },
                { VKIngestMetadataKeys.IngestedAtUtc, timestamp }
            };

            if (context.CollectionName is not null)
            {
                enriched[VKIngestMetadataKeys.CollectionName] = context.CollectionName;
            }

            return VKResult.Success<IReadOnlyDictionary<string, object>>(enriched);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<IReadOnlyDictionary<string, object>>(VKError.Failure(
                "AI.Ingest.Enrichment.Failed",
                $"Failed to enrich metadata: {ex.Message}")); // [CS.01] Map exception to VKResult
        }
    }
}
