using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Pipeline.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pipeline.Internal;

/// <summary>
/// Default implementation of <see cref="IVKPsychePipeline"/>.
/// </summary>
internal sealed class DefaultPsychePipeline : IVKPsychePipeline
{
    private readonly IVKPsychePipelineExecutor _executor;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DefaultPsychePipeline> _logger;
    private readonly IServiceProvider _services;

    public DefaultPsychePipeline(
        IVKPsychePipelineExecutor executor,
        IVKGuidGenerator guidGenerator,
        TimeProvider timeProvider,
        ILogger<DefaultPsychePipeline> logger,
        IServiceProvider services)
    {
        _executor = VKGuard.NotNull(executor);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _timeProvider = VKGuard.NotNull(timeProvider);
        _logger = VKGuard.NotNull(logger);
        _services = VKGuard.NotNull(services);
    }

    public async Task<VKResult<VKPsycheResponse>> ExecuteAsync(
        VKPsycheRequest request,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(request);

        var stopwatch = Stopwatch.StartNew();
        var traceId = request.CorrelationId ?? _guidGenerator.Create().ToString();
        var now = _timeProvider.GetUtcNow();

        _logger.PipelineStarted(
            string.Join(",", request.PersonaIds),
            request.SessionId,
            traceId);

        var context = new VKPsycheContext
        {
            Request = request,
            CorrelationId = traceId,
            CreatedAt = request.CreatedAt ?? now,
            Services = _services
        };

        var result = await _executor.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);

        var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        PipelineDiagnostics.RecordPipelineExecution(elapsedMs, result.IsSuccess);

        if (result.IsFailure)
        {
            _logger.PipelineFailed(
                traceId,
                result.FirstError.Code,
                result.FirstError.Description);

            return result;
        }

        _logger.PipelineCompleted(traceId, elapsedMs);

        return result;
    }
}
