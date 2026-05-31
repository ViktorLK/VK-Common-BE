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
            if (currentChunk == null)
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

    public static async Task<VKResult> ExecuteChunksAsync<T>(
        List<List<T>> chunks,
        VKWeavingContext context,
        Func<T, bool> isParallelSelector,
        Func<T, VKWeavingContext, CancellationToken, Task<VKResult>> executeFunc,
        Func<VKWeavingContext, bool> shouldContinueFunc,
        Action<VKWeavingContext, VKResult> onFailureAction,
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
