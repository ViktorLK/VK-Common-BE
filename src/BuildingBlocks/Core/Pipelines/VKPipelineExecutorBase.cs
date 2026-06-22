using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Abstract base class implementing the generic pipeline execution algorithm.
/// Coordinates the execution flow of: Before stages -> Middleware onion chain -> After stages.
/// Follows AP.01 and CS.03.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class VKPipelineExecutorBase<TContext, TResponse> : IVKPipelineExecutor<TContext, TResponse>
    where TContext : class
{
    private readonly List<List<IVKBeforePipelineStage<TContext>>> _beforeChunks;
    private readonly List<List<IVKAfterPipelineStage<TContext>>> _afterChunks;
    private readonly List<IVKMiddleware<TContext, TResponse>> _middlewares;

    /// <summary>
    /// Initializes a new instance of <see cref="VKPipelineExecutorBase{TContext, TResponse}"/>.
    /// </summary>
    protected VKPipelineExecutorBase(
        IEnumerable<IVKBeforePipelineStage<TContext>> beforeStages,
        IEnumerable<IVKAfterPipelineStage<TContext>> afterStages,
        IEnumerable<IVKMiddleware<TContext, TResponse>> middlewares)
    {
        VKGuard.NotNull(beforeStages);
        VKGuard.NotNull(afterStages);

        _beforeChunks = VKPipelineRunner.ChunkStages(
            beforeStages.Where(s => s.IsActive),
            s => s.Schedule.StageOrder,
            s => s.Schedule.ParallelGroup);

        _afterChunks = VKPipelineRunner.ChunkStages(
            afterStages.Where(s => s.IsActive),
            s => s.Schedule.StageOrder,
            s => s.Schedule.ParallelGroup);

        _middlewares = VKGuard.NotNull(middlewares)
            .OrderBy(m => m.MiddlewareOrder)
            .ToList();
    }

    /// <summary>
    /// Invokes the terminal action (e.g., the actual LLM engine call).
    /// </summary>
    protected abstract Task<VKResult<TResponse>> InvokeTerminalAsync(TContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Checks if the execution context has been marked as aborted.
    /// </summary>
    protected abstract bool CheckAborted(TContext context);

    /// <summary>
    /// Gets the failure result when aborted.
    /// </summary>
    protected abstract VKResult GetAbortResult(TContext context);

    /// <inheritdoc />
    public virtual async Task<VKResult<TResponse>> ExecuteAsync(TContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // 1. Run BEFORE stages
        var beforeResult = await VKPipelineRunner.ExecuteChunksAsync(
            _beforeChunks,
            context,
            CheckAborted,
            GetAbortResult,
            s => s.Schedule.IsParallel,
            (s, ctx, ct) => s.ExecuteAsync(ctx, ct),
            cancellationToken).ConfigureAwait(false);

        if (beforeResult.IsFailure)
        {
            return VKResult.Failure<TResponse>(beforeResult.Errors);
        }

        if (CheckAborted(context))
        {
            var abortResult = GetAbortResult(context);
            return VKResult.Failure<TResponse>(abortResult.Errors);
        }

        // 2. Build the middleware delegate onion chain starting from terminalAction
        VKPipelineDelegate<TResponse> chain = () => InvokeTerminalAsync(context, cancellationToken);

        // Wrap middlewares in reverse order (onion style)
        for (int i = _middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = _middlewares[i];
            var currentNext = chain;
            chain = () => middleware.InvokeAsync(context, currentNext, cancellationToken);
        }

        var middlewareResult = await chain().ConfigureAwait(false);
        if (middlewareResult.IsFailure)
        {
            return middlewareResult;
        }

        // 3. Run AFTER stages
        var afterResult = await VKPipelineRunner.ExecuteChunksAsync(
            _afterChunks,
            context,
            CheckAborted,
            GetAbortResult,
            s => s.Schedule.IsParallel,
            (s, ctx, ct) => s.ExecuteAsync(ctx, ct),
            cancellationToken).ConfigureAwait(false);

        if (afterResult.IsFailure)
        {
            return VKResult.Failure<TResponse>(afterResult.Errors);
        }

        return middlewareResult;
    }
}
