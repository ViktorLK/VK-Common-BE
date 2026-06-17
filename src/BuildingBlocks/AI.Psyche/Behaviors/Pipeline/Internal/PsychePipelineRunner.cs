using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Behaviors.Pipeline.Internal;

/// <summary>
/// Helper to chunk and execute pipeline stages (Before/After) safely in Psyche.
/// Handles parallel group execution and fail-fast control.
/// </summary>
internal static class PsychePipelineRunner
{
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

    public static async Task<VKResult> ExecuteChunksAsync<T>(
        List<List<T>> chunks,
        VKPsycheContext context,
        Func<T, bool> isParallelSelector,
        Func<T, VKPsycheContext, CancellationToken, Task<VKResult>> executeFunc,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(chunks);
        VKGuard.NotNull(context);
        VKGuard.NotNull(isParallelSelector);
        VKGuard.NotNull(executeFunc);

        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (context.IsAborted)
            {
                return VKResult.Failure(VKBehaviorsErrors.Aborted);
            }

            var parallel = chunk.Where(isParallelSelector).ToList();
            var serial = chunk.Where(s => !isParallelSelector(s)).ToList();

            // Run same-layer/same-order parallel stages concurrently
            if (parallel.Count > 0)
            {
                var tasks = parallel.Select(s => executeFunc(s, context, cancellationToken)).ToList();
                var results = await Task.WhenAll(tasks).ConfigureAwait(false); // [CS.03]

                foreach (var result in results)
                {
                    if (result.IsFailure)
                    {
                        return result; // Fail fast on any failure
                    }
                }
            }

            // Run serial stages sequentially
            foreach (var stage in serial)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (context.IsAborted)
                {
                    return VKResult.Failure(VKBehaviorsErrors.Aborted);
                }

                var result = await executeFunc(stage, context, cancellationToken).ConfigureAwait(false); // [CS.03]
                if (result.IsFailure)
                {
                    return result; // Fail fast on failure
                }
            }
        }

        return VKResult.Success();
    }
}
