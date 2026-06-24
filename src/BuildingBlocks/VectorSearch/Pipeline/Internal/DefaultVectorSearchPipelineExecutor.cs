using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Common.Diagnostics.Internal;

namespace VK.Blocks.VectorSearch.Pipeline.Internal;

/// <summary>
/// Default implementation of the Vector Search pipeline executor.
/// Inherits from <see cref="VKPipelineExecutorBase{TContext, TResponse}"/> and handles the terminal Search execution.
/// </summary>
internal sealed class DefaultVectorSearchPipelineExecutor : VKPipelineExecutorBase<VKVectorSearchContext, VKSearchResult[]>, IVKVectorSearchPipelineExecutor
{
    private readonly ILogger<DefaultVectorSearchPipelineExecutor> _logger;

    public DefaultVectorSearchPipelineExecutor(
        IEnumerable<IVKVectorSearchBeforePipelineStage> beforeStages,
        IEnumerable<IVKVectorSearchAfterPipelineStage> afterStages,
        IEnumerable<IVKVectorSearchMiddleware> middlewares,
        ILogger<DefaultVectorSearchPipelineExecutor> logger)
        : base(beforeStages, afterStages, middlewares)
    {
        _logger = VKGuard.NotNull(logger);
    }

    public override async Task<VKResult<VKSearchResult[]>> ExecuteAsync(
        VKVectorSearchContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        _logger.PipelineStarted(context.Query.Text);
        var stopwatch = Stopwatch.StartNew();

        var result = await base.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

        stopwatch.Stop();

        if (result.IsFailure)
        {
            _logger.PipelineFailed(result.FirstError.ToString());
            return result;
        }

        _logger.PipelineCompleted(result.Value.Length, stopwatch.Elapsed.TotalMilliseconds);

        return result;
    }

    protected override async Task<VKResult<VKSearchResult[]>> InvokeTerminalAsync(
        VKVectorSearchContext context,
        CancellationToken cancellationToken)
    {
        if (context.State<SemanticCacheHitState>()?.IsHit == true)
        {
            _logger.CacheHitBypassed();
            return VKResult.Success(context.Results);
        }

        if (context.Services.GetService(typeof(IVKSearchStrategy)) is not IVKSearchStrategy searchStrategy)
        {
            return VKResult.Failure<VKSearchResult[]>(VKVectorSearchPipelineErrors.SearchStrategyNotFound);
        }

        var searchResult = await searchStrategy.SearchAsync(context.Query, cancellationToken).ConfigureAwait(false);
        if (searchResult.IsFailure)
        {
            return VKResult.Failure<VKSearchResult[]>(searchResult.Errors);
        }

        context.Results = searchResult.Value;
        return VKResult.Success(searchResult.Value);
    }

    protected override bool CheckAborted(VKVectorSearchContext context) => context.IsAborted;

    protected override VKResult GetAbortResult(VKVectorSearchContext context) => VKResult.Failure(VKVectorSearchPipelineErrors.Aborted);
}
