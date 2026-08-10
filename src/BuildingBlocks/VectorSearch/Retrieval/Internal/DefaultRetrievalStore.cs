using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorSearch.Retrieval.Internal;

internal sealed class DefaultRetrievalStore : IVKRetrievalStore
{
    private readonly IVKVectorStore _vectorStore;
    private readonly IVKIdentityContext _identityContext;
    private readonly VKVectorStoreOptions _defaults;

    public DefaultRetrievalStore(
        IVKVectorStore vectorStore,
        IVKIdentityContext identityContext,
        Microsoft.Extensions.Options.IOptions<VKVectorStoreOptions> defaultsOptions)
    {
        _vectorStore = VKGuard.NotNull(vectorStore);
        _identityContext = VKGuard.NotNull(identityContext);
        _defaults = defaultsOptions?.Value ?? new VKVectorStoreOptions();
    }

    public async Task<VKResult> UpsertAsync(
        IEnumerable<VKDocumentChunk> chunks,
        IEnumerable<VKVector> embeddings,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(chunks);
        VKGuard.NotNull(embeddings);

        var chunkList = chunks.ToList();
        var vectorList = embeddings.ToList();

        if (chunkList.Count != vectorList.Count)
        {
            return VKResult.Failure(VKError.Failure("VectorSearch.Mismatch", "Chunk and vector counts do not match."));
        }

        var collection = _vectorStore.Collection<VectorStoreDocument>(_defaults.DefaultCollection);

        foreach (var (chunk, vector) in chunkList.Zip(vectorList))
        {
            var doc = new VectorStoreDocument(chunk.Content, chunk.Metadata);
            var result = await collection.UpsertAsync(chunk.Id, doc, vector, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
            {
                return result;
            }
        }

        return VKResult.Success();
    }

    public async Task<VKResult<IEnumerable<VKVectorSearchResult>>> SearchAsync(
        VKVector embedding,
        VKVectorSearchArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(embedding);

        var searchArgs = args ?? new VKVectorSearchArgs
        {
            TenantId = _identityContext.TenantId,
            Limit = _defaults.DefaultLimit,
            MinScore = (float)_defaults.DefaultMinScore
        };

        var collection = _vectorStore.Collection<VectorStoreDocument>(_defaults.DefaultCollection);
        var result = await collection.SearchAsync(embedding, searchArgs, cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            return VKResult.Failure<IEnumerable<VKVectorSearchResult>>(result.Errors);
        }

        var mapped = result.Value.Select(r => new VKVectorSearchResult
        {
            Chunk = new VKDocumentChunk
            {
                Id = r.Id,
                Content = r.Document.Content,
                DocumentId = r.Document.Metadata.Properties.TryGetValue("DocumentId", out var docId) ? docId : "Unknown",
                Metadata = r.Document.Metadata
            },
            Score = (float)r.Score
        });

        return VKResult.Success(mapped);
    }

    internal sealed record VectorStoreDocument(string Content, VKVectorMetadata Metadata);
}
