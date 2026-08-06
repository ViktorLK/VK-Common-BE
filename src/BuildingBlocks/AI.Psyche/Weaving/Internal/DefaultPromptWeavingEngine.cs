using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

/// <summary>
/// Weaving task engine implementation utilizing Core <see cref="VKPipelineRunner"/> for task execution.
/// </summary>
internal sealed class DefaultPromptWeavingEngine : IVKWeavingTaskEngine
{
    private readonly IEnumerable<IVKWeavingTask> _tasks;
    private readonly VKWeavingOptions _options;

    public DefaultPromptWeavingEngine(
        IEnumerable<IVKWeavingTask> tasks,
        VKWeavingOptions options)
    {
        _tasks = VKGuard.NotNull(tasks);
        _options = VKGuard.NotNull(options);
    }

    public async Task<VKResult> WeavePromptAsync(
        VKPsycheContext context,
        CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        // Early pruning of disabled tiers so that downstream formatting & truncation tasks ignore them
        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _options.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Count > 0)
        {
            var activeFragments = context.Fragments
                .Where(f => !disabledTiers.Contains(f.TierType))
                .ToList();
            context.SetFragments(activeFragments);
        }

        var sortedTasks = _tasks.OrderBy(t => t.Schedule.Order).ToList();
        var chunks = VKPipelineRunner.ChunkStages(
            sortedTasks,
            t => t.Schedule.Order,
            t => t.Schedule.ParallelGroup);

        var runResult = await VKPipelineRunner.ExecuteChunksAsync(
            chunks,
            context,
            checkAbortedFunc: ctx => false,
            abortResultFunc: ctx => VKResult.Failure(VKWeavingErrors.NoTapestry),
            isParallelSelector: t => t.Schedule.IsParallel,
            executeFunc: (t, ctx, ct) => t.ExecuteAsync(ctx, ct),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (runResult.IsFailure)
        {
            return runResult;
        }

        if (context.Response.Messages.Count == 0)
        {
            return VKResult.Failure(VKWeavingErrors.NoTapestry);
        }

        return VKResult.Success();
    }
}
