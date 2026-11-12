using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorSearch.Retrieval.Internal;

/// <summary>
/// Dense Vector based search strategy delegating directly to <see cref="IVKVectorStore"/>.
/// Follows AP.01, CS.01, CS.03.
/// </summary>
internal sealed class DefaultVectorSearchStrategy : IVKSearchStrategy
{
    private readonly IVKEmbeddingsEngine _embeddingsEngine;
    private readonly IVKVectorStore _vectorStore;
    private readonly IVKTenantCoordinate _tenantCoordinate;
    private readonly VKRetrievalOptions _options;
    private readonly VKVectorStoreOptions _vectorStoreDefaults;
    private readonly IVKJsonSerializer _jsonSerializer;

    public DefaultVectorSearchStrategy(
        IVKEmbeddingsEngine embeddingsEngine,
        IVKVectorStore vectorStore,
        IVKTenantCoordinate tenantCoordinate,
        IOptions<VKRetrievalOptions> options,
        IOptions<VKVectorStoreOptions> vectorStoreDefaults,
        IVKJsonSerializer jsonSerializer)
    {
        _embeddingsEngine = VKGuard.NotNull(embeddingsEngine);
        _vectorStore = VKGuard.NotNull(vectorStore);
        _tenantCoordinate = VKGuard.NotNull(tenantCoordinate);
        _options = options.Value;
        _vectorStoreDefaults = vectorStoreDefaults.Value;
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
    }

    public async Task<VKResult<VKSearchResult[]>> SearchAsync(VKSearchQuery query, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(query);

        var embeddingResult = await _embeddingsEngine.GenerateAsync(query.Text, cancellationToken).ConfigureAwait(false);
        if (embeddingResult.IsFailure)
        {
            return VKResult.Failure<VKSearchResult[]>(embeddingResult.Errors);
        }

        var embeddingVector = embeddingResult.Value;
        var collectionName = string.IsNullOrWhiteSpace(query.CollectionName)
            ? _vectorStoreDefaults.DefaultCollection
            : query.CollectionName;

        var args = new VKVectorSearchArgs
        {
            TenantId = _tenantCoordinate.TenantId,
            Limit = query.TopK,
            MinScore = query.Threshold.HasValue ? (float)query.Threshold.Value : 0.0f,
            CollectionName = collectionName
        };

        var collection = _vectorStore.Collection<VKVectorDocument>(collectionName);
        var searchResult = await collection.SearchAsync(embeddingVector, args, cancellationToken).ConfigureAwait(false);
        if (searchResult.IsFailure)
        {
            return VKResult.Failure<VKSearchResult[]>(searchResult.Errors);
        }

        var mapped = searchResult.Value.Select(r => new VKSearchResult
        {
            Document = new VKDocument
            {
                Id = r.Document.Metadata.Properties.TryGetValue("document_id", out var docId) 
                     ? docId 
                     : r.Document.Metadata.Properties.TryGetValue("DocumentId", out var docIdPascal)
                         ? docIdPascal
                         : r.Id,
                Content = r.Document.Content,
                Metadata = _jsonSerializer.Serialize(r.Document.Metadata)
            },
            Score = (float)r.Score
        }).ToArray();

        return VKResult.Success(mapped);
    }
}
