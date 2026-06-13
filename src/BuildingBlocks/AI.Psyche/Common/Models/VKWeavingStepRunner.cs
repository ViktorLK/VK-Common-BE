using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

// // [AP.03] Shared Foundation internal helper without VK prefix using root flat namespace
internal static class VKWeavingStepRunner
{
    /// <summary>
    /// Groups a sequence of weaving steps into chunks based on execution order and parallelization groups.
    /// </summary>
    /// <typeparam name="T">The type of the step.</typeparam>
    /// <param name="steps">The sequence of steps to group.</param>
    /// <param name="orderSelector">A function to extract the absolute order of a step.</param>
    /// <param name="parallelGroupSelector">A function to extract the parallel group ID of a step, or null if it must run serially.</param>
    /// <returns>A list of chunks, where each chunk contains steps that can run concurrently or must run serially in the same order band.</returns>
    public static List<List<T>> ChunkSteps<T>(
        IEnumerable<T> steps,
        Func<T, int> orderSelector,
        Func<T, int?> parallelGroupSelector)
    {
        // [AP.01] Defensive Programming via VKGuard
        VKGuard.NotNull(steps);
        VKGuard.NotNull(orderSelector);
        VKGuard.NotNull(parallelGroupSelector);

        var sorted = steps.OrderBy(orderSelector).ToList();
        var chunks = new List<List<T>>();
        List<T>? currentChunk = null;

        foreach (var step in sorted)
        {
            if (currentChunk is null)
            {
                currentChunk = [step];
                chunks.Add(currentChunk);
            }
            else
            {
                var prev = currentChunk.Last();
                var currentGroup = parallelGroupSelector(step);
                var prevGroup = parallelGroupSelector(prev);
                var currentOrder = orderSelector(step);
                var prevOrder = orderSelector(prev);

                if ((currentGroup.HasValue && currentGroup == prevGroup) || currentOrder == prevOrder)
                {
                    currentChunk.Add(step);
                }
                else
                {
                    currentChunk = [step];
                    chunks.Add(currentChunk);
                }
            }
        }

        return chunks;
    }

    /// <summary>
    /// Executes the chunked weaving steps safely, supporting both parallel and serial execution within a chunk.
    /// </summary>
    /// <typeparam name="T">The type of the step.</typeparam>
    /// <param name="chunks">The list of step chunks to execute.</param>
    /// <param name="context">The weaving context shared across all steps.</param>
    /// <param name="isParallelSelector">A function determining if a specific step in a chunk can be executed in parallel.</param>
    /// <param name="executeFunc">The function to execute a single step asynchronously.</param>
    /// <param name="shouldContinueFunc">A function evaluated before each step/chunk to determine if execution should proceed.</param>
    /// <param name="onFailureAction">An action invoked if a step execution returns a failure result.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A successful result if all steps succeed; otherwise, the first failure result encountered.</returns>
    public static async Task<VKResult> ExecuteChunksAsync<T>(
        List<List<T>> chunks,
        VKPsycheContext context,
        Func<T, bool> isParallelSelector,
        Func<T, VKPsycheContext, CancellationToken, Task<VKResult>> executeFunc,
        Func<VKPsycheContext, bool> shouldContinueFunc,
        Action<VKPsycheContext, VKResult> onFailureAction,
        CancellationToken cancellationToken)
    {
        // [AP.01] Defensive Programming via VKGuard
        VKGuard.NotNull(chunks);
        VKGuard.NotNull(context);
        VKGuard.NotNull(isParallelSelector);
        VKGuard.NotNull(executeFunc);
        VKGuard.NotNull(shouldContinueFunc);
        VKGuard.NotNull(onFailureAction);

        foreach (var chunk in chunks)
        {
            if (!shouldContinueFunc(context))
            {
                break;
            }

            var parallel = chunk.Where(isParallelSelector).ToList();
            var serial = chunk.Where(s => !isParallelSelector(s)).ToList();

            // Run same-layer/same-order parallel steps concurrently
            if (parallel.Count > 0)
            {
                var tasks = parallel.Select(s => executeFunc(s, context, cancellationToken)).ToList();
                var results = await Task.WhenAll(tasks).ConfigureAwait(false); // // [CS.03]

                foreach (var result in results)
                {
                    if (result.IsFailure)
                    {
                        onFailureAction(context, result);
                        if (!shouldContinueFunc(context))
                        {
                            return result;
                        }
                    }
                }
            }

            // Run serial steps sequentially
            foreach (var step in serial)
            {
                if (!shouldContinueFunc(context))
                {
                    break;
                }

                var result = await executeFunc(step, context, cancellationToken).ConfigureAwait(false); // // [CS.03]
                if (result.IsFailure)
                {
                    onFailureAction(context, result);
                    if (!shouldContinueFunc(context))
                    {
                        return result;
                    }
                }
            }
        }

        return VKResult.Success();
    }
}
