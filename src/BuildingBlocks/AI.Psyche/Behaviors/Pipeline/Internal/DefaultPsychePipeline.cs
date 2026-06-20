using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Behaviors.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Behaviors.Pipeline.Internal;

/// <summary>
/// Re-implemented DefaultPsychePipeline that delegates prompt weaving to the unified IVKPsychePipelineExecutor.
/// Implements AP.01 (sealed class default) and CS.03.
/// </summary>
internal sealed class DefaultPsychePipeline : IVKPsychePipeline
{
    private readonly IVKPsychePipelineExecutor _behaviorExecutor;
    private readonly IServiceProvider _services;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly ILogger<DefaultPsychePipeline> _logger;

    public DefaultPsychePipeline(
        IVKPsychePipelineExecutor behaviorExecutor,
        IServiceProvider services,
        IVKGuidGenerator guidGenerator,
        ILogger<DefaultPsychePipeline> logger)
    {
        _behaviorExecutor = VKGuard.NotNull(behaviorExecutor);
        _services = VKGuard.NotNull(services);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKPsycheResponse>> RunAsync(
        VKPsycheRequest request,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(request); // [AP.01]

        var stopwatch = Stopwatch.StartNew();
        var traceId = request.CorrelationId ?? _guidGenerator.Create().ToString();

        _logger.PipelineStarted(
            request.PersonaId,
            request.SessionId,
            traceId);

        // Create the unified weaving context
        var weavingContext = new VKPsycheContext
        {
            Request = string.IsNullOrWhiteSpace(request.CorrelationId)
                ? request with { CorrelationId = traceId }
                : request,
            Services = _services
        };

        // Execute via the behaviors pipeline executor
        var executeResult = await _behaviorExecutor.ExecuteAsync(weavingContext, cancellationToken).ConfigureAwait(false); // [CS.03]

        stopwatch.Stop();

        if (executeResult.IsFailure)
        {
            _logger.PipelineFailed(
                traceId,
                executeResult.FirstError.Code,
                executeResult.FirstError.Description);

            return VKResult.Failure<VKPsycheResponse>(executeResult.Errors); // [CS.01]
        }

        _logger.PipelineCompleted(traceId, stopwatch.Elapsed.TotalMilliseconds);

        return executeResult;
    }
}
