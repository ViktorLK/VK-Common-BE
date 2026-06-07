using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Pipeline.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pipeline.Internal;

/// <summary>
/// Default implementation of the general psyche pipeline.
/// Implements AP.01 (sealed class default) and CS.03 (ConfigureAwait(false) on all awaits).
/// </summary>
internal sealed class DefaultPsychePipeline : IVKPsychePipeline
{
    private readonly IEnumerable<IVKPsychePipelineStage> _stages;
    private readonly IVKGuidGenerator _guidGenerator;
    private readonly ILogger<DefaultPsychePipeline> _logger;

    public DefaultPsychePipeline(
        IEnumerable<IVKPsychePipelineStage> stages,
        IVKGuidGenerator guidGenerator,
        ILogger<DefaultPsychePipeline> logger)
    {
        _stages = VKGuard.NotNull(stages);
        _guidGenerator = VKGuard.NotNull(guidGenerator);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKPromptTapestry>> WeaveTapestryAsync(
        VKWeavingRequest request,
        CancellationToken cancellationToken = default)
    {
        // // [AP.01] Boundary checks via VKGuard
        VKGuard.NotNull(request);

        var stopwatch = Stopwatch.StartNew();

        // Convert external Request into library-internal execution Context
        var context = new VKWeavingContext
        {
            PersonaId = request.PersonaId,
            SessionId = request.SessionId,
            UserInput = request.UserInput,
            Args = request.Args,
            CorrelationId = request.CorrelationId ?? _guidGenerator.Create().ToString(),
            Echo = request.Echo,
            Knowledge = request.Knowledge,
            Persona = request.Persona,
            Directive = request.Directive,
        };

        PipelineDiagnostics.PipelineStarted(
            _logger,
            context.PersonaId,
            context.SessionId,
            context.CorrelationId);

        // 2. Resolve Active stages and construct parallel chunks
        var targetStages = _stages
            .Where(s => s.IsActive)
            .ToList();

        var chunks = VKWeavingStepRunner.ChunkSteps(
            targetStages,
            s => s.StageOrder,
            s => s.ParallelGroup);

        bool hasFailed = false;
        VKResult? failedResult = null;

        // Execute stages in order with fail-fast execution control
        await VKWeavingStepRunner.ExecuteChunksAsync(
            chunks,
            context,
            s => s.IsParallel,
            (s, ctx, ct) => s.ExecuteAsync(ctx, ct),
            ctx => !hasFailed,
            (ctx, res) =>
            {
                hasFailed = true;
                failedResult = res;
            },
            cancellationToken).ConfigureAwait(false); // // [CS.03]

        stopwatch.Stop();

        if (hasFailed && failedResult != null)
        {
            PipelineDiagnostics.PipelineFailed(
                _logger,
                context.CorrelationId,
                failedResult.FirstError.Code,
                failedResult.FirstError.Description);

            return VKResult.Failure<VKPromptTapestry>(failedResult.Errors); // // [CS.01]
        }

        PipelineDiagnostics.PipelineCompleted(_logger, context.CorrelationId, stopwatch.Elapsed.TotalMilliseconds);

        if (context.Tapestry is null)
        {
            return VKResult.Failure<VKPromptTapestry>(VKPipelineErrors.EmptyTapestry);
        }

        return VKResult.Success(context.Tapestry);
    }
}
