using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Pipelines.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pipelines.Internal;

/// <summary>
/// Default implementation of <see cref="IVKPsychePipeline"/>.
/// </summary>
internal sealed class DefaultPsychePipeline : IVKPsychePipeline
{
    private readonly IVKPsychePipelineExecutor _executor;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly ILogger<DefaultPsychePipeline> _logger;
    private readonly IServiceProvider _services;

    public DefaultPsychePipeline(
        IVKPsychePipelineExecutor executor,
        IVKGuidGenerator guidGenerator,
        ILogger<DefaultPsychePipeline> logger,
        IServiceProvider services)
    {
        _executor = VKGuard.NotNull(executor);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _logger = VKGuard.NotNull(logger);
        _services = VKGuard.NotNull(services);
    }

    public async Task<VKResult<VKPsycheResponse>> RunAsync(
        VKPsycheRequest request,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(request);

        var stopwatch = Stopwatch.StartNew();
        var traceId = request.CorrelationId ?? _guidGenerator.Create().ToString();

        _logger.PipelineStarted(
            request.PersonaId,
            request.SessionId,
            traceId);

        var requestWithTrace = string.IsNullOrWhiteSpace(request.CorrelationId)
            ? request with { CorrelationId = traceId }
            : request;

        var context = new VKPsycheContext
        {
            Request = requestWithTrace,
            Services = _services
        };

        var result = await _executor.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

        stopwatch.Stop();

        if (result.IsFailure)
        {
            _logger.PipelineFailed(
                traceId,
                result.FirstError.Code,
                result.FirstError.Description);

            return result;
        }

        _logger.PipelineCompleted(traceId, stopwatch.Elapsed.TotalMilliseconds);

        return result;
    }
}
