using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Abstract base class implementing the generic pipeline execution algorithm.
/// Coordinates the execution flow of: Before components -> Middleware onion chain -> After components.
/// Automatically filters components into Before (or unspecified None) and After phases based on component.Schedule.Phase.
/// Follows AP.01 and CS.03.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public abstract class VKPipelineExecutorBase<TContext, TResponse> : IVKPipelineExecutor<TContext, TResponse>
    where TContext : class
{
    private readonly List<List<IVKPipelineComponent<TContext>>> _beforeChunks;
    private readonly List<List<IVKPipelineComponent<TContext>>> _afterChunks;
    private readonly List<IVKMiddleware<TContext>> _middlewares;

    /// <summary>
    /// Initializes a new instance of <see cref="VKPipelineExecutorBase{TContext, TResponse}"/>.
    /// Components are automatically split into Before (None or Before) and After phases using component.Schedule.Phase.
    /// </summary>
    protected VKPipelineExecutorBase(
        IEnumerable<IVKPipelineComponent<TContext>> components,
        IEnumerable<IVKMiddleware<TContext>> middlewares)
    {
        VKGuard.NotNull(components);

        var componentList = components.ToList();

        // Components with Phase == Before or None (default) run before terminal action
        _beforeChunks = VKPipelineRunner.ChunkStages(
            componentList.Where(c => c.Schedule.Phase != VKPipelinePhase.After),
            c => c.Schedule.Order,
            c => c.Schedule.ParallelGroup);

        // Components explicitly marked with Phase == After run after terminal action
        _afterChunks = VKPipelineRunner.ChunkStages(
            componentList.Where(c => c.Schedule.Phase == VKPipelinePhase.After),
            c => c.Schedule.Order,
            c => c.Schedule.ParallelGroup);

        _middlewares = VKGuard.NotNull(middlewares)
            .OrderBy(m => m.MiddlewareOrder)
            .ToList();
    }

    /// <summary>
    /// Invokes the terminal action (e.g., the actual LLM engine call).
    /// </summary>
    protected abstract Task<VKResult> InvokeTerminalAsync(TContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Builds the final immutable TResponse from the context state after all stages run.
    /// </summary>
    protected abstract TResponse BuildResponse(TContext context);

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

        // 1. Run BEFORE components (Phase == Before or None)
        var beforeResult = await VKPipelineRunner.ExecuteChunksAsync(
            _beforeChunks,
            context,
            CheckAborted,
            GetAbortResult,
            c => c.Schedule.IsParallel,
            (c, ctx, ct) => c.ExecuteAsync(ctx, ct),
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
        VKPipelineDelegate chain = () => InvokeTerminalAsync(context, cancellationToken);

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
            return VKResult.Failure<TResponse>(middlewareResult.Errors);
        }

        // 3. Run AFTER components (Phase == After)
        var afterResult = await VKPipelineRunner.ExecuteChunksAsync(
            _afterChunks,
            context,
            CheckAborted,
            GetAbortResult,
            c => c.Schedule.IsParallel,
            (c, ctx, ct) => c.ExecuteAsync(ctx, ct),
            cancellationToken).ConfigureAwait(false);

        if (afterResult.IsFailure)
        {
            return VKResult.Failure<TResponse>(afterResult.Errors);
        }

        return VKResult.Success(BuildResponse(context));
    }
}
