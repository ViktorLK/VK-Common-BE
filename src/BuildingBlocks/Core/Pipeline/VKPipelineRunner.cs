using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Universal execution engine for pipeline components (<see cref="IVKPipelineComponent{TContext, TResult}"/>).
/// Supports ordered sequential execution, optional parallel group execution, short-circuiting, and fail-fast control.
/// </summary>
public static class VKPipelineRunner
{
    /// <summary>
    /// Chunks components into execution groups based on ordering and parallel group selectors.
    /// </summary>
    public static List<List<T>> ChunkStages<T>(
        IEnumerable<T> stages,
        Func<T, int> orderSelector,
        Func<T, int?> parallelGroupSelector)
    {
        VKGuard.NotNull(stages);
        VKGuard.NotNull(orderSelector);
        VKGuard.NotNull(parallelGroupSelector);

        var sorted = stages.OrderBy(orderSelector).ToList();
        var chunks = new List<List<T>>();
        List<T>? currentChunk = null;

        foreach (var stage in sorted)
        {
            if (currentChunk is null)
            {
                currentChunk = [stage];
                chunks.Add(currentChunk);
            }
            else
            {
                var prev = currentChunk.Last();
                var currentGroup = parallelGroupSelector(stage);
                var prevGroup = parallelGroupSelector(prev);
                var currentOrder = orderSelector(stage);
                var prevOrder = orderSelector(prev);

                if ((currentGroup.HasValue && currentGroup == prevGroup) || currentOrder == prevOrder)
                {
                    currentChunk.Add(stage);
                }
                else
                {
                    currentChunk = [stage];
                    chunks.Add(currentChunk);
                }
            }
        }

        return chunks;
    }

    /// <summary>
    /// Legacy helper to execute chunked stages for backwards compatibility.
    /// </summary>
    public static async Task<VKResult> ExecuteChunksAsync<T, TContext>(
        List<List<T>> chunks,
        TContext context,
        Func<TContext, bool> checkAbortedFunc,
        Func<TContext, VKResult> abortResultFunc,
        Func<T, bool> isParallelSelector,
        Func<T, TContext, CancellationToken, Task<VKResult>> executeFunc,
        CancellationToken cancellationToken) where TContext : class
    {
        VKGuard.NotNull(chunks);
        VKGuard.NotNull(context);
        VKGuard.NotNull(checkAbortedFunc);
        VKGuard.NotNull(abortResultFunc);
        VKGuard.NotNull(isParallelSelector);
        VKGuard.NotNull(executeFunc);

        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (checkAbortedFunc(context))
            {
                return abortResultFunc(context);
            }

            var parallel = chunk.Where(isParallelSelector).ToList();
            var serial = chunk.Where(s => !isParallelSelector(s)).ToList();

            var activeParallel = parallel.Where(s => s is not IVKPipelineComponent { IsActive: false }).ToList();
            if (activeParallel.Count > 0)
            {
                var tasks = activeParallel.Select(s => executeFunc(s, context, cancellationToken)).ToList();
                var results = await Task.WhenAll(tasks).ConfigureAwait(false);

                foreach (var result in results)
                {
                    if (result.IsFailure)
                    {
                        return result;
                    }
                }
            }

            foreach (var stage in serial)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (checkAbortedFunc(context))
                {
                    return abortResultFunc(context);
                }

                if (stage is IVKPipelineComponent { IsActive: false })
                {
                    continue;
                }

                var result = await executeFunc(stage, context, cancellationToken).ConfigureAwait(false);
                if (result.IsFailure)
                {
                    return result;
                }
            }
        }

        return VKResult.Success();
    }

    /// <summary>
    /// Universal component execution engine. Evaluates any collection of <see cref="IVKPipelineComponent{TContext, TResult}"/>.
    /// Algorithm sorts by Schedule.Order and applies decoupled options (short-circuiting, aborting, parallelization).
    /// </summary>
    public static async Task<VKResult<TResult>> ExecuteComponentsAsync<TContext, TResult>(
        IEnumerable<IVKPipelineComponent<TContext, TResult>> components,
        TContext context,
        VKPipelineComponentOptions<TContext, TResult>? options = null,
        TResult defaultResult = default!,
        CancellationToken cancellationToken = default) where TContext : class
    {
        VKGuard.NotNull(components);
        VKGuard.NotNull(context);

        var sortedComponents = components.OrderBy(c => c.Schedule.Order).ToList();
        if (sortedComponents.Count == 0)
        {
            return VKResult.Success(defaultResult);
        }

        foreach (var component in sortedComponents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!component.IsActive)
            {
                continue;
            }

            if (options?.AbortPredicate is not null && options.AbortPredicate(context))
            {
                return options.AbortResultFactory is not null
                    ? options.AbortResultFactory(context)
                    : VKResult.Failure<TResult>(VKError.Failure("Core.Pipeline.Aborted", "Pipeline execution was aborted."));
            }

            var result = await component.ExecuteAsync(context, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (result.IsFailure)
            {
                return result;
            }

            if (options?.ShortCircuitPredicate is not null && options.ShortCircuitPredicate(result.Value))
            {
                return result;
            }
        }

        return VKResult.Success(defaultResult);
    }

    /// <summary>
    /// Universal component execution engine for non-generic void components (<see cref="IVKPipelineComponent{TContext}"/>).
    /// Evaluates any collection of components with ordering, parallel grouping, and chunking.
    /// </summary>
    public static Task<VKResult> ExecuteComponentsAsync<TContext>(
        IEnumerable<IVKPipelineComponent<TContext>> components,
        TContext context,
        Func<TContext, bool>? checkAbortedFunc = null,
        Func<TContext, VKResult>? abortResultFunc = null,
        CancellationToken cancellationToken = default) where TContext : class
    {
        VKGuard.NotNull(components);
        VKGuard.NotNull(context);

        var sorted = components.OrderBy(c => c.Schedule.Order).ToList();
        if (sorted.Count == 0)
        {
            return Task.FromResult(VKResult.Success());
        }

        var chunks = ChunkStages(
            sorted,
            c => c.Schedule.Order,
            c => c.Schedule.ParallelGroup);

        return ExecuteChunksAsync(
            chunks,
            context,
            checkAbortedFunc: checkAbortedFunc ?? (_ => false),
            abortResultFunc: abortResultFunc ?? (_ => VKResult.Success()),
            isParallelSelector: c => c.Schedule.IsParallel,
            executeFunc: (c, ctx, ct) => c.ExecuteAsync(ctx, ct),
            cancellationToken: cancellationToken);
    }
}
