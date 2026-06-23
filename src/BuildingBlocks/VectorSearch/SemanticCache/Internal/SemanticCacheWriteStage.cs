using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Pipeline.Internal;
using VK.Blocks.VectorSearch.Common.Diagnostics.Internal;

namespace VK.Blocks.VectorSearch.SemanticCache.Internal;

/// <summary>
/// Pipeline stage for writing results back to the semantic cache after pipeline execution.
/// </summary>
internal sealed class SemanticCacheWriteStage : IVKVectorSearchAfterPipelineStage
{
    private readonly IVKSemanticCacheService _cacheService;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly VKSemanticCacheOptions _options;
    private readonly ILogger<SemanticCacheWriteStage> _logger;

    public SemanticCacheWriteStage(
        IVKSemanticCacheService cacheService,
        IVKJsonSerializer jsonSerializer,
        IOptions<VKSemanticCacheOptions> options,
        ILogger<SemanticCacheWriteStage> logger)
    {
        _cacheService = VKGuard.NotNull(cacheService);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public bool IsActive => _options.Enabled;

    public VKPipelineStageSchedule Schedule => VKVectorSearchPipelineScheduler.After.SemanticCacheWrite;

    public async Task<VKResult> ExecuteAsync(VKVectorSearchContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        // Skip writing if the results are already from a cache hit
        if (context.State<SemanticCacheHitState>()?.IsHit == true)
        {
            return VKResult.Success();
        }

        if (context.Results.Length == 0)
        {
            return VKResult.Success();
        }

        var json = _jsonSerializer.Serialize(context.Results);
        var cacheResult = await _cacheService.SetAsync(context.Query.Text, json, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (cacheResult.IsFailure)
        {
            _logger.CacheWriteFailed(cacheResult.FirstError.ToString());
        }

        return VKResult.Success();
    }
}
