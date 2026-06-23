using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Pipeline.Internal;
using VK.Blocks.VectorSearch.Common.Diagnostics.Internal;

namespace VK.Blocks.VectorSearch.SemanticCache.Internal;

/// <summary>
/// Pipeline stage for querying the semantic cache before executing backend search.
/// </summary>
internal sealed class DefaultSemanticCacheStage : IVKVectorSearchBeforePipelineStage
{
    private readonly IVKSemanticCacheService _cacheService;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly VKSemanticCacheOptions _options;
    private readonly ILogger<DefaultSemanticCacheStage> _logger;

    public DefaultSemanticCacheStage(
        IVKSemanticCacheService cacheService,
        IVKJsonSerializer jsonSerializer,
        IOptions<VKSemanticCacheOptions> options,
        ILogger<DefaultSemanticCacheStage> logger)
    {
        _cacheService = VKGuard.NotNull(cacheService);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public bool IsActive => _options.Enabled;

    public VKPipelineStageSchedule Schedule => VKVectorSearchPipelineScheduler.Before.SemanticCache;

    public async Task<VKResult> ExecuteAsync(VKVectorSearchContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        // Retrieve from semantic cache
        var cacheResult = await _cacheService.GetAsync(context.Query.Text, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (cacheResult.IsFailure)
        {
            _logger.CacheRetrievalFailed(cacheResult.FirstError.ToString());
            return VKResult.Success(); // Continue search on cache error to ensure robustness
        }

        if (cacheResult.Value is not null)
        {
            var cachedResults = _jsonSerializer.Deserialize<VKSearchResult[]>(cacheResult.Value);
            if (cachedResults is not null)
            {
                _logger.CacheHit(context.Query.Text);
                context.Results = cachedResults;
                context.SetState(new SemanticCacheHitState(true));
            }
        }

        return VKResult.Success();
    }
}
