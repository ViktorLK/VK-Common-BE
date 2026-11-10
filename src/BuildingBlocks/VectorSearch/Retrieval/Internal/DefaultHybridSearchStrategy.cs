using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Fusion.Internal;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorSearch.Retrieval.Internal;

/// <summary>
/// Hybrid search strategy merging keyword and vector results directly via <see cref="IVKVectorStore"/>.
/// </summary>
internal sealed class DefaultHybridSearchStrategy : IVKSearchStrategy
{
    private readonly IVKSparseSearchEngine _sparseSearchEngine;
    private readonly IVKEmbeddingsEngine _embeddingsEngine;
    private readonly IVKVectorStore _vectorStore;
    private readonly IVKTenantCoordinate _tenantCoordinate;
    private readonly VKRetrievalOptions _options;
    private readonly VKVectorStoreOptions _vectorStoreDefaults;
    private readonly IVKScoreFusion _rrfFusion = new ReciprocalRankFusion();

    public DefaultHybridSearchStrategy(
        IVKSparseSearchEngine sparseSearchEngine,
        IVKEmbeddingsEngine embeddingsEngine,
        IVKVectorStore vectorStore,
        IVKTenantCoordinate tenantCoordinate,
        IOptions<VKRetrievalOptions> options,
        IOptions<VKVectorStoreOptions> vectorStoreDefaults)
    {
        _sparseSearchEngine = VKGuard.NotNull(sparseSearchEngine);
        _embeddingsEngine = VKGuard.NotNull(embeddingsEngine);
        _vectorStore = VKGuard.NotNull(vectorStore);
        _tenantCoordinate = VKGuard.NotNull(tenantCoordinate);
        _options = VKGuard.NotNull(options?.Value);
        _vectorStoreDefaults = VKGuard.NotNull(vectorStoreDefaults?.Value);
    }

    public async Task<VKResult<VKSearchResult[]>> SearchAsync(VKSearchQuery query, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(query);
        VKGuard.NotNullOrWhiteSpace(query.Text);

        var embeddingResult = await _embeddingsEngine.GenerateAsync(query.Text, cancellationToken).ConfigureAwait(false);
        if (embeddingResult.IsFailure)
        {
            return VKResult.Failure<VKSearchResult[]>(embeddingResult.Errors);
        }

        var embeddingVector = embeddingResult.Value;
        var collectionName = string.IsNullOrWhiteSpace(query.CollectionName)
            ? _vectorStoreDefaults.DefaultCollection
            : query.CollectionName;

        var searchArgs = new VKVectorSearchArgs
        {
            TenantId = _tenantCoordinate.TenantId,
            Limit = query.TopK,
            MinScore = query.Threshold.HasValue ? (float)query.Threshold.Value : 0.0f,
            CollectionName = collectionName
        };

        // Runtime Capability Check: If the vector store natively supports hybrid search, delegate directly to it.
        if (_vectorStore is IVKHybridCapableVectorStore hybridCapableStore)
        {
            var hybridResult = await hybridCapableStore.SearchHybridAsync<VKVectorDocument>(
                collectionName,
                embeddingVector,
                query.Text,
                searchArgs,
                cancellationToken).ConfigureAwait(false);

            if (hybridResult.IsFailure)
            {
                return VKResult.Failure<VKSearchResult[]>(hybridResult.Errors);
            }

            var mapped = hybridResult.Value.Select(r => new VKSearchResult
            {
                Document = new VKDocument
                {
                    Id = r.Document.Metadata.Properties.TryGetValue("document_id", out var docId)
                        ? docId
                        : r.Document.Metadata.Properties.TryGetValue("DocumentId", out var docIdPascal)
                            ? docIdPascal
                            : r.Id,
                    Content = r.Document.Content,
                    Metadata = r.Document.Metadata.Properties.TryGetValue("DocumentMetadata", out var docMeta) ? docMeta : string.Empty
                },
                Score = (float)r.Score
            }).ToArray();

            return VKResult.Success(mapped);
        }

        // Parallelize Vector and Sparse (Keyword) search
        var collection = _vectorStore.Collection<VKVectorDocument>(collectionName);
        var vectorSearchTask = collection.SearchAsync(embeddingVector, searchArgs, cancellationToken);
        var sparseSearchTask = _sparseSearchEngine.SearchAsync(query, cancellationToken);

        await Task.WhenAll(vectorSearchTask, sparseSearchTask).ConfigureAwait(false);

        var vectorResult = await vectorSearchTask.ConfigureAwait(false);
        var sparseResult = await sparseSearchTask.ConfigureAwait(false);

        if (vectorResult.IsFailure)
        {
            return VKResult.Failure<VKSearchResult[]>(vectorResult.Errors);
        }
        if (sparseResult.IsFailure)
        {
            return VKResult.Failure<VKSearchResult[]>(sparseResult.Errors);
        }

        var denseCandidates = vectorResult.Value.Select((r, idx) => new VKFusionCandidate
        {
            Chunk = new VKDocumentChunk
            {
                Id = r.Id,
                Content = r.Document.Content,
                DocumentId = r.Document.Metadata.Properties.TryGetValue("document_id", out var dId)
                    ? dId
                    : r.Document.Metadata.Properties.TryGetValue("DocumentId", out var dIdPascal)
                        ? dIdPascal
                        : r.Id,
                Metadata = r.Document.Metadata
            },
            Score = (float)r.Score,
            Rank = idx + 1
        }).ToList();

        var sparseCandidates = sparseResult.Value.Select((r, idx) => new VKFusionCandidate
        {
            Chunk = r.Chunk,
            Score = r.Score,
            Rank = idx + 1
        }).ToList();

        // Select fusion strategy dynamically based on options.
        var scoreFusion = _options.HybridFusionStrategy == VKFusionStrategy.WeightedScore
            ? new WeightedScoreFusion(_options.HybridWeights)
            : _rrfFusion;

        var fusionResult = scoreFusion.Fuse([denseCandidates, sparseCandidates], query.TopK);
        if (fusionResult.IsFailure)
        {
            return VKResult.Failure<VKSearchResult[]>(fusionResult.Errors);
        }

        return VKResult.Success(fusionResult.Value.ToArray());
    }
}
