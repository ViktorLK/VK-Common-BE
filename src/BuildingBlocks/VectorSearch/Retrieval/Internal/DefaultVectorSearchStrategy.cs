using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorSearch.Retrieval.Internal;

/// <summary>
/// Dense Vector based search strategy.
/// </summary>
internal sealed class DefaultVectorSearchStrategy : IVKSearchStrategy
{
    private readonly IVKRetrievalStore _retrievalStore;
    private readonly IVKEmbeddingsEngine _embeddingsEngine;
    private readonly IVKVectorStore _vectorStore;
    private readonly IVKUserContext _userContext;
    private readonly IVKJsonSerializer _jsonSerializer;

    public DefaultVectorSearchStrategy(
        IVKRetrievalStore retrievalStore,
        IVKEmbeddingsEngine embeddingsEngine,
        IVKVectorStore vectorStore,
        IVKUserContext userContext,
        IVKJsonSerializer jsonSerializer)
    {
        _retrievalStore = VKGuard.NotNull(retrievalStore);
        _embeddingsEngine = VKGuard.NotNull(embeddingsEngine);
        _vectorStore = VKGuard.NotNull(vectorStore);
        _userContext = VKGuard.NotNull(userContext);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
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
        var args = new VKVectorSearchArgs
        {
            TenantId = _userContext.TenantId ?? "Default",
            Limit = query.TopK,
            MinScore = query.Threshold.HasValue ? (float)query.Threshold.Value : 0.0f,
            CollectionName = query.CollectionName
        };

        // Check if the store supports native hybrid path for potential acceleration
        if (_vectorStore is IVKHybridCapableVectorStore)
        {
            // Accelerated path could be executed here if configured, 
            // but for pure vector search we fall back to standard collection search.
        }

        var searchResult = await _retrievalStore.SearchAsync(embeddingVector, args, cancellationToken).ConfigureAwait(false);
        if (searchResult.IsFailure)
        {
            return VKResult.Failure<VKSearchResult[]>(searchResult.Errors);
        }

        var mapped = searchResult.Value.Select(r => new VKSearchResult
        {
            Document = new VKDocument
            {
                Id = r.Chunk.DocumentId,
                Content = r.Chunk.Content,
                Metadata = _jsonSerializer.Serialize(r.Chunk.Metadata)
            },
            Score = r.Score
        }).ToArray();

        return VKResult.Success(mapped);
    }
}
