using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorStore;

namespace VK.Blocks.VectorSearch.SemanticCache.Internal;

/// <summary>
/// Production-grade implementation of IVKSemanticCacheService.
/// </summary>
internal sealed class DefaultSemanticCacheService : IVKSemanticCacheService
{
    private readonly IVKVectorStore _vectorStore;
    private readonly IVKEmbeddingsEngine _embeddingsEngine;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly IVKUserContext _userContext;
    private readonly VKSemanticCacheOptions _options;
    private readonly TimeProvider _timeProvider;

    public DefaultSemanticCacheService(
        IVKVectorStore vectorStore,
        IVKEmbeddingsEngine embeddingsEngine,
        IVKGuidGenerator guidGenerator,
        IVKUserContext userContext,
        IOptions<VKSemanticCacheOptions> options,
        TimeProvider? timeProvider = null)
    {
        _vectorStore = VKGuard.NotNull(vectorStore);
        _embeddingsEngine = VKGuard.NotNull(embeddingsEngine);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _userContext = VKGuard.NotNull(userContext);
        _options = VKGuard.NotNull(options?.Value);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<VKResult<string?>> GetAsync(string prompt, CancellationToken cancellationToken = default)
    {
        // [RuleID: AP.01]
        VKGuard.NotNullOrWhiteSpace(prompt);

        if (!_options.Enabled)
        {
            return VKResult.Success<string?>(null);
        }

        var embeddingResult = await _embeddingsEngine.GenerateAsync(prompt, cancellationToken).ConfigureAwait(false);
        if (embeddingResult.IsFailure)
        {
            return VKResult.Failure<string?>(embeddingResult.Errors);
        }

        var vector = embeddingResult.Value;
        var collection = _vectorStore.Collection<CacheEntry>(_options.CollectionName);

        var searchArgs = new VKVectorSearchArgs
        {
            TenantId = _userContext.TenantId ?? "Default",
            Limit = 1,
            MinScore = (float)_options.ScoreThreshold
        };

        var searchResult = await collection.SearchAsync(vector, searchArgs, cancellationToken).ConfigureAwait(false);
        if (searchResult.IsFailure)
        {
            return VKResult.Failure<string?>(searchResult.Errors);
        }

        var match = searchResult.Value.FirstOrDefault();
        if (match is null)
        {
            return VKResult.Success<string?>(null);
        }

        // Verify expiration
        var age = _timeProvider.GetUtcNow() - match.Document.CreatedAt;
        if (age > _options.Ttl)
        {
            // Lazily delete expired entry
            _ = collection.DeleteAsync(match.Id, cancellationToken: cancellationToken);
            return VKResult.Success<string?>(null);
        }

        // Sliding Expiration: Refresh the expiration time on cache hit
        if (_options.SlidingExpiration)
        {
            var refreshedEntry = new CacheEntry
            {
                Content = match.Document.Content,
                Response = match.Document.Response,
                CreatedAt = _timeProvider.GetUtcNow()
            };
            _ = collection.UpsertAsync(match.Id, refreshedEntry, vector, cancellationToken);
        }

        return VKResult.Success<string?>(match.Document.Response);
    }

    public async Task<VKResult> SetAsync(string prompt, string response, CancellationToken cancellationToken = default)
    {
        // [RuleID: AP.01]
        VKGuard.NotNullOrWhiteSpace(prompt);
        VKGuard.NotNull(response);

        if (!_options.Enabled)
        {
            return VKResult.Success();
        }

        var embeddingResult = await _embeddingsEngine.GenerateAsync(prompt, cancellationToken).ConfigureAwait(false);
        if (embeddingResult.IsFailure)
        {
            return embeddingResult;
        }

        var vector = embeddingResult.Value;
        var collection = _vectorStore.Collection<CacheEntry>(_options.CollectionName);

        // Generate sequential ID using IVKGuidGenerator [CS.06]
        var id = _guidGenerator.Create().ToString();
        var entry = new CacheEntry
        {
            Content = prompt,
            Response = response,
            CreatedAt = _timeProvider.GetUtcNow()
        };

        var upsertResult = await collection.UpsertAsync(id, entry, vector, cancellationToken).ConfigureAwait(false);
        if (upsertResult.IsFailure)
        {
            return upsertResult;
        }

        return VKResult.Success();
    }

    internal sealed class CacheEntry
    {
        public required string Content { get; init; }
        public required string Response { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
    }
}
