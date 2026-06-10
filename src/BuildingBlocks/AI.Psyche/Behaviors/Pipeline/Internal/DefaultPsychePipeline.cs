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

        BehaviorsDiagnostics.PipelineStarted(
            _logger,
            request.PersonaId,
            request.SessionId,
            traceId);

        // Create the unified weaving context
        var weavingContext = new VKPsycheContext
        {
            PersonaId = request.PersonaId,
            SessionId = request.SessionId,
            UserInput = request.UserInput,
            WeavingArgs = request.Args,
            CorrelationId = traceId,
            EchoArgs = request.Echo,
            KnowledgeArgs = request.Knowledge,
            PersonaArgs = request.Persona,
            DirectiveArgs = request.Directive,
            Services = _services
        };

        // Execute via the behaviors pipeline executor
        var executeResult = await _behaviorExecutor.ExecuteAsync(weavingContext, cancellationToken).ConfigureAwait(false); // [CS.03]

        stopwatch.Stop();

        if (executeResult.IsFailure)
        {
            BehaviorsDiagnostics.PipelineFailed(
                _logger,
                traceId,
                executeResult.FirstError.Code,
                executeResult.FirstError.Description);

            return VKResult.Failure<VKPsycheResponse>(executeResult.Errors); // [CS.01]
        }

        BehaviorsDiagnostics.PipelineCompleted(_logger, traceId, stopwatch.Elapsed.TotalMilliseconds);

        if (weavingContext.Response is null)
        {
            return VKResult.Failure<VKPsycheResponse>(VKBehaviorsErrors.EmptyTapestry);
        }

        return VKResult.Success(weavingContext.Response);
    }
}
