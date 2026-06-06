using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;
using VK.Blocks.AI.SemanticKernel.Vectorics.VectorStore;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Retrieval.Internal;

/// <summary>
/// A Semantic Kernel implementation of a retrieval engine.
/// This implementation leverages <see cref="VectorStore "/> and <see cref="VectorStoreCollection{TKey, TRecord}"/> for modern SK RAG patterns.
/// </summary>
internal sealed class AISKRetrievalEngine(
    Microsoft.SemanticKernel.Kernel kernel,
    IOptions<VKAIDefaultsOptions> globalOptions,
    IOptions<VKRetrievalOptions> options,
    ILogger<AISKRetrievalEngine> logger,
    VectorStore ? vectorStore = null,
    TimeProvider? timeProvider = null)
    : AISKEngineBase<VKRetrievalOptions>(kernel, globalOptions, options, logger, timeProvider), IVKRetrievalEngine
{
    private readonly VectorStoreCollection<string, VKVectorStoreRecord>? _collection =
        vectorStore?.GetCollection<string, VKVectorStoreRecord>("default");

    /// <summary>
    /// Performs a semantic search using the modern SK Vector Store.
    /// </summary>
    public async Task<VKResult<IReadOnlyList<VKRetrievalResult>>> SearchAsync(
        string query,
        VKRetrievalArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(query); // [AP.01]

        if (_collection is null)
        {
            Logger.LogRetrievalMemoryNotRegistered();
            return VKResult.Failure<IReadOnlyList<VKRetrievalResult>>(VKAIErrors.EngineError("VectorStore  is not registered."));
        }

        return await ExecuteAsync(async (ct) =>
        {
            Logger.LogRetrievalSearch(query);

            int topK = FeatureOptions.TopK ?? 5;
            double minScore = FeatureOptions.MinScore ?? 0.7;

            if (args != null)
            {
                topK = args.TopK ?? topK;
                minScore = args.MinScore ?? minScore;
            }

            // Ensure collection is created before query
            await _collection.EnsureCollectionExistsAsync(ct).ConfigureAwait(false); // [CS.03]

            // 1. Generate Query Vector using native SK text embedding generation service
            var embeddingGenerator = Kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(); // [CS.07]
            var embedding = await embeddingGenerator.GenerateAsync(query, cancellationToken: ct).ConfigureAwait(false); // [CS.03]
            var queryVector = embedding.Vector;

            // 2. Query Modern Vector Store
            var searchOptions = new VectorSearchOptions<VKVectorStoreRecord>();

            var searchResults = _collection.SearchAsync(queryVector, topK, searchOptions, ct);

            var results = new List<VKRetrievalResult>();
            await foreach (var r in searchResults.WithCancellation(ct).ConfigureAwait(false)) // [CS.03]
            {
                if (r.Score < minScore)
                {
                    continue;
                }

                results.Add(new VKRetrievalResult
                {
                    Content = r.Record.Text,
                    Score = r.Score ?? 0.0,
                    SourceUrl = r.Record.AdditionalMetadata,
                    Metadata = new Dictionary<string, object?>
                    {
                        ["Id"] = r.Record.Id,
                        ["Description"] = r.Record.Description
                    }
                });
            }

            return (IReadOnlyList<VKRetrievalResult>)results;
        }, args, VKRetrievalErrors.FeatureDisabled, cancellationToken).ConfigureAwait(false); // [CS.03]
    }
}
