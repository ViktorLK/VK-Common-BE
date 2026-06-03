using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Vectorics.VectorStore;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Vectorics.SemanticCache.Internal;

/// <summary>
/// A Semantic Kernel implementation of <see cref="IVKSemanticCache"/>.
/// Leverages <see cref="VectorStore "/> and <see cref="VectorStoreCollection{TKey, TRecord}"/> to store and retrieve semantically similar prompts.
/// </summary>
internal sealed class AISKSemanticCache : IVKSemanticCache
{
    private readonly VectorStoreCollection<string, VKVectorStoreRecord>? _collection;
    private readonly Microsoft.SemanticKernel.Kernel _kernel;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly ILogger<AISKSemanticCache> _logger;

    private const string CacheCollectionName = "semantic-cache";
    private const double DefaultSimilarityThreshold = 0.95;

    public AISKSemanticCache(
        Microsoft.SemanticKernel.Kernel kernel,
        IVKGuidGenerator guidGenerator,
        ILogger<AISKSemanticCache> logger,
        Microsoft.Extensions.VectorData.VectorStore? vectorStore = null)
    {
        _kernel = VKGuard.NotNull(kernel); // [AP.01]
        _guidGenerator = VKGuard.NotNull(guidGenerator); // [AP.01]
        _logger = VKGuard.NotNull(logger); // [AP.01]

        if (vectorStore is not null)
        {
            _collection = vectorStore.GetCollection<string, VKVectorStoreRecord>(CacheCollectionName);
        }
    }

    /// <inheritdoc />
    public async Task<VKResult<string>> GetAsync(string prompt, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(prompt); // [AP.01]

        if (_collection is null)
        {
            _logger.LogSemanticCacheNotRegistered();
            return VKResult.Failure<string>(VKAIErrors.EngineError("Semantic Cache is not configured."));
        }
        int topK =  5;

        try
        {
            await _collection.EnsureCollectionDeletedAsync(cancellationToken).ConfigureAwait(false); // [CS.03]

            // Generate Prompt Vector using native SK text embedding service
            var embeddingGenerator = _kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(); // [CS.07]
            var embedding = await embeddingGenerator.GenerateAsync([prompt], cancellationToken: cancellationToken).ConfigureAwait(false); // [CS.03]

            // 2. Query Modern Vector Store
            var searchOptions = new VectorSearchOptions<VKVectorStoreRecord>();

            var searchResults = _collection.SearchAsync(embedding, topK, searchOptions, cancellationToken);

            await foreach (var result in searchResults.WithCancellation(cancellationToken).ConfigureAwait(false)) // [CS.03]
            {
                if (result.Score >= DefaultSimilarityThreshold)
                {
                    var cachedResponse = result.Record.AdditionalMetadata;
                    if (!string.IsNullOrWhiteSpace(cachedResponse))
                    {
                        _logger.LogSemanticCacheHit(result.Score ?? 0.0);
                        return VKResult.Success(cachedResponse);
                    }
                }
            }

            _logger.LogSemanticCacheMiss();
            return VKResult.Failure<string>(new VKError("CacheMiss", "No semantically similar response found in cache."));
        }
        catch (Exception ex)
        {
            _logger.LogSemanticCacheRetrievalError(ex);
            return VKResult.Failure<string>(VKAIErrors.EngineError(ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<VKResult> SetAsync(string prompt, string response, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(prompt); // [AP.01]
        VKGuard.NotNullOrWhiteSpace(response); // [AP.01]

        if (_collection is null)
        {
            return VKResult.Failure(VKAIErrors.EngineError("Semantic Cache is not configured."));
        }

        try
        {
            await _collection.EnsureCollectionDeletedAsync(cancellationToken).ConfigureAwait(false); // [CS.03]

            // Generate Prompt Vector using native SK text embedding service
            var embeddingGenerator = _kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(); // [CS.07]
            var embedding = await embeddingGenerator.GenerateAsync([prompt], cancellationToken: cancellationToken).ConfigureAwait(false); // [CS.03]

            var record = new VKVectorStoreRecord
            {
                Id = _guidGenerator.Create().ToString(), // Complies with CS.06 via injected generator
                CollectionName = CacheCollectionName,
                Text = prompt,
                Description = "Semantic Cache Entry",
                AdditionalMetadata = response,
                Embedding = null // TODO
            };

            await _collection.UpsertAsync(record, cancellationToken: cancellationToken).ConfigureAwait(false); // [CS.03]

            _logger.LogSemanticCacheSaved();
            return VKResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogSemanticCacheSaveError(ex);
            return VKResult.Failure(VKAIErrors.EngineError(ex.Message));
        }
    }
}
