using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Common.Diagnostics.Internal;

namespace VK.Blocks.VectorSearch.Pipeline.Internal;

/// <summary>
/// Default implementation of the Vector Search pipeline.
/// </summary>
internal sealed class DefaultVectorSearchPipeline : IVKVectorSearchPipeline
{
    private readonly IVKVectorSearchPipelineExecutor _executor;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly ILogger<DefaultVectorSearchPipeline> _logger;
    private readonly IServiceProvider _services;

    public DefaultVectorSearchPipeline(
        IVKVectorSearchPipelineExecutor executor,
        IVKGuidGenerator guidGenerator,
        ILogger<DefaultVectorSearchPipeline> logger,
        IServiceProvider services)
    {
        _executor = VKGuard.NotNull(executor);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _logger = VKGuard.NotNull(logger);
        _services = VKGuard.NotNull(services);
    }

    public async Task<VKResult<VKSearchResult[]>> RunAsync(
        VKSearchQuery request,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(request);

        var stopwatch = Stopwatch.StartNew();
        var traceId = request.CorrelationId ?? _guidGenerator.Create().ToString();

        _logger.PipelineStartedWithTrace(traceId);

        var requestWithTrace = string.IsNullOrWhiteSpace(request.CorrelationId)
            ? request with { CorrelationId = traceId }
            : request;

        var context = new VKVectorSearchContext
        {
            Query = requestWithTrace,
            Services = _services
        };

        var result = await _executor.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

        stopwatch.Stop();

        if (result.IsFailure)
        {
            _logger.PipelineFailedWithTrace(traceId, result.FirstError.ToString());
            return result;
        }

        _logger.PipelineCompletedWithTrace(traceId, stopwatch.Elapsed.TotalMilliseconds);

        return result;
    }
}
