using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Engram.Memory.Diagnostics.Internal;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch;
using VK.Blocks.VectorStore;

namespace VK.Blocks.AI.Engram.Memory.Internal;

/// <summary>
/// Default implementation of <see cref="IVKMemorySearchService"/>.
/// Executes single-pass vector searches using payload copies when semantic query is supplied,
/// or falls back to raw <see cref="IVKMemoryStore"/> scope queries.
/// </summary>
internal sealed class DefaultMemorySearchService : IVKMemorySearchService
{
    private readonly IVKMemoryStore _memoryStore;
    private readonly IVKIdentityContext _identityContext;
    private readonly VKMemoryOptions _options;
    private readonly ILogger<DefaultMemorySearchService> _logger;
    private readonly IVKRetrievalStore? _retrievalStore;
    private readonly IVKEmbeddingsEngine? _embeddingsEngine;

    public DefaultMemorySearchService(
        IVKMemoryStore memoryStore,
        IVKIdentityContext identityContext,
        Microsoft.Extensions.Options.IOptions<VKMemoryOptions> options,
        ILogger<DefaultMemorySearchService> logger,
        IVKRetrievalStore? retrievalStore = null,
        IVKEmbeddingsEngine? embeddingsEngine = null)
    {
        _memoryStore = VKGuard.NotNull(memoryStore);
        _identityContext = VKGuard.NotNull(identityContext);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
        _retrievalStore = retrievalStore;
        _embeddingsEngine = embeddingsEngine;
    }

    public async Task<VKResult<IEnumerable<VKMemoryQueryResult>>> SearchAsync(
        VKMemoryQuery query,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        VKGuard.NotNull(query);

        if (!_options.Enabled)
        {
            return VKResult.Success<IEnumerable<VKMemoryQueryResult>>([]);
        }

        var effectiveTopK = query.TopK > 0 ? query.TopK : (_options.DefaultTopK ?? 5);
        var effectiveMinScore = query.MinScore > 0f ? query.MinScore : (_options.DefaultMinScore ?? 0.7f);
        var targetTenantId = query.TenantId ?? _identityContext.TenantId;

        // If a semantic query is present and vector search capabilities are registered, perform real embedding + vector search
        if (!string.IsNullOrWhiteSpace(query.SemanticQuery) &&
            _retrievalStore is not null &&
            _embeddingsEngine is not null)
        {
            // 1. Generate real mathematical query vector via IVKEmbeddingsEngine
            var embeddingResult = await _embeddingsEngine.GenerateAsync(query.SemanticQuery, cancellationToken).ConfigureAwait(false);
            if (embeddingResult.IsFailure)
            {
                return VKResult.Failure<IEnumerable<VKMemoryQueryResult>>(embeddingResult.Errors);
            }

            var searchArgs = new VKVectorSearchArgs
            {
                Limit = effectiveTopK,
                MinScore = effectiveMinScore,
                TenantId = targetTenantId.Value
            };

            // 2. Perform vector search in RetrievalStore
            var searchResult = await _retrievalStore.SearchAsync(embeddingResult.Value, searchArgs, cancellationToken).ConfigureAwait(false);

            if (searchResult.IsSuccess)
            {
                var queryResults = searchResult.Value.Select(r => new VKMemoryQueryResult
                {
                    Entry = new VKMemoryEntry
                    {
                        Id = new VKMemoryId(Guid.TryParse(r.Chunk.Id, out var parsedGuid) ? parsedGuid : Guid.NewGuid()),
                        Content = r.Chunk.Content,
                        TenantId = targetTenantId,
                        CreatedAt = DateTimeOffset.UtcNow,
                        Category = query.Category ?? VKMemoryCategory.ShortTerm,
                        ExtendedScope = query.ExtendedScope
                    },
                    Score = r.Score
                }).ToList();

                _logger.MemorySearchCompleted(queryResults.Count, query.SemanticQuery);
                return VKResult.Success<IEnumerable<VKMemoryQueryResult>>(queryResults);
            }
        }

        // Fallback: Query raw entries directly from MemoryStore (Source of Truth)
        var effectiveQuery = query.TopK > 0 ? query : query with { TopK = effectiveTopK };
        var rawResult = await _memoryStore.QueryAsync(effectiveQuery, cancellationToken).ConfigureAwait(false);
        if (rawResult.IsFailure)
        {
            return VKResult.Failure<IEnumerable<VKMemoryQueryResult>>(rawResult.Errors);
        }

        var results = rawResult.Value.Select(e => new VKMemoryQueryResult
        {
            Entry = e,
            Score = 1.0f
        }).ToList();

        _logger.MemorySearchCompleted(results.Count, query.SemanticQuery ?? string.Empty);
        return VKResult.Success<IEnumerable<VKMemoryQueryResult>>(results);
    }
}
