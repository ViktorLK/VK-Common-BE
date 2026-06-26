using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Pipeline.Internal;

namespace VK.Blocks.VectorSearch.Compression.Internal;

/// <summary>
/// Pipeline stage for compressing search results.
/// </summary>
internal sealed class DefaultContextCompressionStage : IVKVectorSearchAfterPipelineStage
{
    private readonly IVKContextCompressionStrategy _strategy;
    private readonly VKContextCompressionOptions _options;

    public DefaultContextCompressionStage(IVKContextCompressionStrategy strategy, IOptions<VKContextCompressionOptions> options)
    {
        _strategy = VKGuard.NotNull(strategy);
        _options = VKGuard.NotNull(options?.Value);
    }

    public bool IsActive => _options.Enabled;

    public VKPipelineStageSchedule Schedule => VKVectorSearchPipelineScheduler.After.ContextCompression;

    public async Task<VKResult> ExecuteAsync(VKVectorSearchContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        if (context.State<SemanticCacheHitState>()?.IsHit == true)
        {
            return VKResult.Success();
        }

        if (context.Results.Length == 0)
        {
            return VKResult.Success();
        }

        var queryText = context.Query.Text;
        if (string.IsNullOrWhiteSpace(queryText))
        {
            return VKResult.Success();
        }

        var compressionResult = await _strategy.CompressContextAsync(context.Results, queryText, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (compressionResult.IsFailure)
        {
            return VKResult.Failure(compressionResult.Errors);
        }

        context.Results = compressionResult.Value;
        return VKResult.Success();
    }
}
