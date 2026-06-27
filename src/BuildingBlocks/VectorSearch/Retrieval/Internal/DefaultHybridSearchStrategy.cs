using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;
using VK.Blocks.VectorSearch.Fusion.Internal;

namespace VK.Blocks.VectorSearch.Retrieval.Internal;

/// <summary>
/// Hybrid search strategy merging keyword and vector results.
/// </summary>
internal sealed class DefaultHybridSearchStrategy : IVKSearchStrategy
{
    private readonly IVKRetrievalStore _retrievalStore;
    private readonly IVKSparseSearchEngine _sparseSearchEngine;
    private readonly IVKEmbeddingsEngine _embeddingsEngine;
    private readonly IVKVectorStore _vectorStore;
    private readonly IVKUserContext _userContext;
    private readonly VKRetrievalOptions _options;
    private readonly VKVectorStoreDefaultsOptions _vectorStoreDefaults;
    private readonly IVKScoreFusion _rrfFusion = new ReciprocalRankFusion();

    public DefaultHybridSearchStrategy(
        IVKRetrievalStore retrievalStore,
        IVKSparseSearchEngine sparseSearchEngine,
        IVKEmbeddingsEngine embeddingsEngine,
        IVKVectorStore vectorStore,
        IVKUserContext userContext,
        IOptions<VKRetrievalOptions> options,
        IOptions<VKVectorStoreDefaultsOptions> vectorStoreDefaults)
    {
        _retrievalStore = VKGuard.NotNull(retrievalStore);
        _sparseSearchEngine = VKGuard.NotNull(sparseSearchEngine);
        _embeddingsEngine = VKGuard.NotNull(embeddingsEngine);
        _vectorStore = VKGuard.NotNull(vectorStore);
        _userContext = VKGuard.NotNull(userContext);
        _options = VKGuard.NotNull(options?.Value);
        _vectorStoreDefaults = VKGuard.NotNull(vectorStoreDefaults?.Value);
    }

    public async Task<VKResult<VKSearchResult[]>> SearchAsync(VKSearchQuery query, CancellationToken cancellationToken = default)
    {
        // [RuleID: AP.01]
        VKGuard.NotNull(query);

        var embeddingResult = await _embeddingsEngine.GenerateAsync(query.Text, cancellationToken).ConfigureAwait(false);
        if (embeddingResult.IsFailure)
        {
            return VKResult.Failure<VKSearchResult[]>(embeddingResult.Errors);
        }

        var embeddingVector = embeddingResult.Value;
        var searchArgs = new VKVectorSearchArgs
        {
            TenantId = _userContext.TenantId ?? "Default",
            Limit = query.TopK,
            MinScore = query.Threshold.HasValue ? (float)query.Threshold.Value : 0.0f,
            CollectionName = query.CollectionName
        };

        // Runtime Capability Check: If the vector store natively supports hybrid search, delegate directly to it.
        if (_vectorStore is IVKHybridCapableVectorStore hybridCapableStore)
        {
            var hybridResult = await hybridCapableStore.SearchHybridAsync<DefaultRetrievalStore.VectorStoreDocument>(
                _vectorStoreDefaults.DefaultCollection,
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
                    Id = r.Document.Metadata.Properties.TryGetValue("DocumentId", out var docId) ? docId : "Unknown",
                    Content = r.Document.Content,
                    Metadata = r.Document.Metadata.Properties.TryGetValue("DocumentMetadata", out var docMeta) ? docMeta : string.Empty
                },
                Score = (float)r.Score
            }).ToArray();

            return VKResult.Success(mapped);
        }

        // Parallelize Vector and Sparse (Keyword) search to optimize I/O waiting time.
        var vectorSearchTask = _retrievalStore.SearchAsync(embeddingVector, searchArgs, cancellationToken);
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
            Chunk = r.Chunk,
            Score = r.Score,
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
