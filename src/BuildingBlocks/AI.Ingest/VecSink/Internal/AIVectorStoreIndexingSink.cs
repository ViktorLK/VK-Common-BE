using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.VectorStore;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest.VecSink.Internal;

/// <summary>
/// A document format designed to bridge RAG chunks to the Vector Store database.
/// </summary>
internal sealed record AIVectorStoreDocument(string Content, VKAIVectorMetadata Metadata);

/// <summary>
/// Implementation of <see cref="IVKVecIndexingSink"/> that writes documents and vectors to an <see cref="IVKAIVectorStore"/>.
/// </summary>
internal sealed class AIVectorStoreIndexingSink(
    IVKAIVectorStore vectorStore,
    IOptions<VKAIVectorStoreDefaultsOptions> defaultsOptions) : IVKVecIndexingSink
{
    private readonly IVKAIVectorStore _vectorStore = VKGuard.NotNull(vectorStore);
    private readonly VKAIVectorStoreDefaultsOptions _defaults = defaultsOptions?.Value ?? new VKAIVectorStoreDefaultsOptions();

    /// <inheritdoc />
    public async Task<VKResult> WriteAsync(
        IEnumerable<VKVecDocumentChunk> chunks,
        IEnumerable<VKEmbeddingsVector> embeddings,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(chunks);
        VKGuard.NotNull(embeddings);

        var chunkList = chunks.ToList();
        var vectorList = embeddings.ToList();

        if (chunkList.Count != vectorList.Count)
        {
            return VKResult.Failure(VKError.Failure("AI.Ingest.Mismatch", "Chunk and vector counts do not match."));
        }

        var collection = _vectorStore.Collection<AIVectorStoreDocument>(_defaults.DefaultCollection);

        foreach (var (chunk, vector) in chunkList.Zip(vectorList))
        {
            var document = new AIVectorStoreDocument(chunk.Content, chunk.Metadata);
            var result = await collection.UpsertAsync(chunk.Id, document, vector, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
            {
                return result;
            }
        }

        return VKResult.Success();
    }
}
