using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Behaviors.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Behaviors.Pipeline.Internal;

/// <summary>
/// Default implementation of IVKPsychePipelineExecutor.
/// Coordinates the execution flow of: Before stages -> Middleware onion chain -> After stages.
/// Follows AP.01 (sealed class default) and CS.03 (ConfigureAwait(false)).
/// </summary>
internal sealed class DefaultPsychePipelineExecutor : IVKPsychePipelineExecutor
{
    private readonly List<List<IVKPsycheBeforePipelineStage>> _beforeChunks;
    private readonly List<List<IVKPsycheAfterPipelineStage>> _afterChunks;
    private readonly List<IVKPsycheMiddleware> _middlewares;
    private readonly ILogger<DefaultPsychePipelineExecutor> _logger;

    public DefaultPsychePipelineExecutor(
        IEnumerable<IVKPsycheBeforePipelineStage> beforeStages,
        IEnumerable<IVKPsycheAfterPipelineStage> afterStages,
        IEnumerable<IVKPsycheMiddleware> middlewares,
        ILogger<DefaultPsychePipelineExecutor> logger)
    {
        VKGuard.NotNull(beforeStages);
        VKGuard.NotNull(afterStages);

        _beforeChunks = PsychePipelineRunner.ChunkStages(
            beforeStages.Where(s => s.IsActive),
            s => s.StageOrder,
            s => s.ParallelGroup);

        _afterChunks = PsychePipelineRunner.ChunkStages(
            afterStages.Where(s => s.IsActive),
            s => s.StageOrder,
            s => s.ParallelGroup);

        _middlewares = VKGuard.NotNull(middlewares)
            .OrderBy(m => m.MiddlewareOrder)
            .ToList();
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<VKPsycheResponse>> ExecuteAsync(
        VKPsycheContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        BehaviorsDiagnostics.ExecutionStarted(_logger, context.SessionId.Value.ToString(), context.CorrelationId);
        var stopwatch = Stopwatch.StartNew();

        // 1. Run BEFORE LLM stages (data gathering, prompt weaving, etc.)
        var beforeResult = await PsychePipelineRunner.ExecuteChunksAsync(
            _beforeChunks,
            context,
            s => s.IsParallel,
            (s, ctx, ct) => s.ExecuteAsync(ctx, ct),
            cancellationToken).ConfigureAwait(false); // [CS.03]

        if (beforeResult.IsFailure)
        {
            BehaviorsDiagnostics.ExecutionFailed(
                _logger,
                context.CorrelationId,
                beforeResult.FirstError.Code,
                beforeResult.FirstError.Description);
            return VKResult.Failure<VKPsycheResponse>(beforeResult.Errors); // [CS.01]
        }

        if (context.IsAborted)
        {
            BehaviorsDiagnostics.ExecutionFailed(
                _logger,
                context.CorrelationId,
                VKBehaviorsErrors.EmptyResponse.Code,
                VKBehaviorsErrors.EmptyResponse.Description);
            return VKResult.Failure<VKPsycheResponse>(VKBehaviorsErrors.EmptyResponse);
        }

        // 2. Build the middleware delegate onion chain starting from the terminal execution.
        VKPsycheMiddlewareDelegate chain = (ctx, ct) =>
        {
            if (ctx.Response != null)
            {
                return Task.FromResult(VKResult.Success(ctx.Response));
            }
            return Task.FromResult(VKResult.Failure<VKPsycheResponse>(VKBehaviorsErrors.EmptyResponse));
        };

        // Wrap middlewares in reverse order (onion style)
        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var currentNext = chain;
            chain = (ctx, ct) => middleware.InvokeAsync(ctx, currentNext, ct);
        }

        var middlewareResult = await chain(context, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (middlewareResult.IsFailure)
        {
            BehaviorsDiagnostics.ExecutionFailed(
                _logger,
                context.CorrelationId,
                middlewareResult.FirstError.Code,
                middlewareResult.FirstError.Description);
            return middlewareResult;
        }

        // 3. Run AFTER LLM stages (response parsing, cleanup, auditing, etc.)
        var afterResult = await PsychePipelineRunner.ExecuteChunksAsync(
            _afterChunks,
            context,
            s => s.IsParallel,
            (s, ctx, ct) => s.ExecuteAsync(ctx, ct),
            cancellationToken).ConfigureAwait(false); // [CS.03]

        stopwatch.Stop();

        if (afterResult.IsFailure)
        {
            BehaviorsDiagnostics.ExecutionFailed(
                _logger,
                context.CorrelationId,
                afterResult.FirstError.Code,
                afterResult.FirstError.Description);
            return VKResult.Failure<VKPsycheResponse>(afterResult.Errors); // [CS.01]
        }

        if (context.Response == null)
        {
            BehaviorsDiagnostics.ExecutionFailed(
                _logger,
                context.CorrelationId,
                VKBehaviorsErrors.EmptyResponse.Code,
                VKBehaviorsErrors.EmptyResponse.Description);
            return VKResult.Failure<VKPsycheResponse>(VKBehaviorsErrors.EmptyResponse);
        }

        BehaviorsDiagnostics.ExecutionCompleted(
            _logger,
            context.CorrelationId,
            stopwatch.Elapsed.TotalMilliseconds);

        return VKResult.Success(context.Response);
    }
}
