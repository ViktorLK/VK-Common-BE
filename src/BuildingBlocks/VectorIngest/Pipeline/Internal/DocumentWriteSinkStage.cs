using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.VectorIngest.Common.Models.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Pipeline.Internal; // [AP.03] Internal namespace

/// <summary>
/// Pipeline stage for enriching, checking duplicates, and writing document chunks/embeddings.
/// </summary>
internal sealed class DocumentWriteSinkStage : IVKIngestPipelineStage // [AP.01] sealed default, [AP.03] internal scoping
{
    private readonly IVKIndexingService _indexingService;
    private readonly IVKChunkMetadataEnricher _metadataEnricher;
    private readonly IVKDeduplicationChecker? _duplicateChecker;

    /// <summary>
    /// Initializes a new instance of <see cref="DocumentWriteSinkStage"/>.
    /// </summary>
    public DocumentWriteSinkStage(
        IVKIndexingService indexingService,
        IVKChunkMetadataEnricher metadataEnricher,
        IVKDeduplicationChecker? duplicateChecker = null)
    {
        _indexingService = VKGuard.NotNull(indexingService); // [AP.01] VKGuard boundary
        _metadataEnricher = VKGuard.NotNull(metadataEnricher);
        _duplicateChecker = duplicateChecker; // Optional dependency
    }

    /// <inheritdoc />
    public VKPipelineStageSchedule Schedule => VKIngestPipelineScheduler.Write;

    /// <inheritdoc />
    public bool IsActive => true;

    /// <inheritdoc />
    public async Task<VKResult> ExecuteAsync(IngestContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context); // [AP.01] VKGuard boundary

        if (context.Chunks.Count == 0)
        {
            return VKResult.Success();
        }

        var enrichmentContext = new VKEnrichmentContext
        {
            DocumentId = context.DocumentId,
            TotalChunks = context.Chunks.Count,
            SourceUri = context.Source,
            DocumentHash = context.DocumentHash ?? string.Empty,
            CollectionName = context.CollectionName
        };

        var itemsToIndex = new List<(VKChunk Chunk, IReadOnlyDictionary<string, object> Metadata)>();

        foreach (var chunk in context.Chunks)
        {
            var enrichResult = _metadataEnricher.Enrich(chunk, enrichmentContext);
            if (enrichResult.IsFailure)
            {
                return enrichResult; // [CS.01] Result only
            }

            var metadata = new Dictionary<string, object>(enrichResult.Value);
            foreach (var kvp in context.CustomMetadata)
            {
                metadata[kvp.Key] = kvp.Value;
            }

            // Optional Deduplication Check
            if (_duplicateChecker is not null)
            {
                var contentHash = metadata[VKIngestMetadataKeys.ContentHash].ToString() ?? string.Empty;
                var dupResult = await _duplicateChecker.IsDuplicateAsync(contentHash, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
                if (dupResult.IsFailure)
                {
                    return dupResult; // [CS.01] Result only
                }

                if (dupResult.Value)
                {
                    // Skip indexing this chunk if it is a duplicate
                    continue;
                }
            }

            itemsToIndex.Add((chunk, metadata));
        }

        if (itemsToIndex.Count > 0)
        {
            var indexResult = await _indexingService.IndexBatchAsync(itemsToIndex, cancellationToken).ConfigureAwait(false); // [CS.03] ConfigureAwait
            if (indexResult.IsFailure)
            {
                return indexResult; // [CS.01] Result only
            }
        }

        return VKResult.Success();
    }
}
