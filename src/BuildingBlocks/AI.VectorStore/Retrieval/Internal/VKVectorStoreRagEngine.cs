using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.AI.VectorStore;
using VK.Blocks.AI.VectorStore.Contracts;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// A DTO used for bridging Retrieval chunks to the generic Vector Store.
/// </summary>
internal sealed record VKVectorDocument(string Content, VKAIVectorMetadata Metadata);

/// <summary>
/// Industrial implementation of AI retrieval store using a Vector Store backend.
/// Acts as a bridge between high-level AI models and low-level Vector Engine.
/// </summary>
internal sealed class VKVectorStoreRagEngine(
    IVKAIVectorStore vectorStore,
    IVKUserContext userContext) : IVKRetrievalStore
{
    private readonly IVKAIVectorStore _vectorStore = VKGuard.NotNull(vectorStore);
    private readonly IVKUserContext _userContext = VKGuard.NotNull(userContext);
    private const string CollectionName = "ai.retrieval";

    public async Task<VKResult> UpsertAsync(
        IEnumerable<VKDocumentChunk> chunks,
        IEnumerable<VKEmbeddingsVector> embeddings,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(chunks);
        VKGuard.NotNull(embeddings);

        var chunkList = chunks.ToList();
        var vectorList = embeddings.ToList();

        if (chunkList.Count != vectorList.Count)
        {
            return VKResult.Failure(VKError.Failure("AI.Retrieval.Mismatch", "Chunk and vector counts do not match."));
        }

        var collection = _vectorStore.Collection<VKVectorDocument>(CollectionName);

        foreach (var (chunk, vector) in chunkList.Zip(vectorList))
        {
            var document = new VKVectorDocument(chunk.Content, chunk.Metadata);
            var result = await collection.UpsertAsync(chunk.Id, document, vector, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
                return result;
        }

        return VKResult.Success();
    }

    public async Task<VKResult<IEnumerable<VKVectorSearchResult>>> SearchAsync(
        VKEmbeddingsVector embedding,
        VKRetrievalArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(embedding);

        var limit = args?.TopK ?? 5;
        var minScore = args?.MinScore ?? 0.7f;
        var tenantId = _userContext.TenantId ?? "Default";

        var searchArgs = new VKAIVectorSearchArgs
        {
            TenantId = tenantId,
            Limit = limit,
            MinScore = minScore
        };

        var collection = _vectorStore.Collection<VKVectorDocument>(CollectionName);
        var result = await collection.SearchAsync(embedding, searchArgs, cancellationToken).ConfigureAwait(false);
        
        if (result.IsFailure)
        {
            return VKResult.Failure<IEnumerable<VKVectorSearchResult>>(result.Errors);
        }

        var mappedResults = result.Value.Select(r => new VKVectorSearchResult
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

        return VKResult.Success(mappedResults);
    }
}
