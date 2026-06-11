using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Psyche.Weaving.Internal;

// [AP.01] sealed default implementation
internal sealed class DefaultPromptWeavingEngine : IVKWeavingTaskEngine
{
    private readonly IEnumerable<IVKWeavingTask> _tasks;
    private readonly VKWeavingOptions _options;

    public DefaultPromptWeavingEngine(
        IEnumerable<IVKWeavingTask> tasks,
        IOptions<VKWeavingOptions> options)
    {
        _tasks = VKGuard.NotNull(tasks);
        _options = VKGuard.NotNull(options).Value;
    }

    public async Task<VKResult<VKPsycheResponse>> WeavePromptAsync(
        VKPsycheContext context,
        CancellationToken cancellationToken)
    {
        // // [AP.01] Defensive boundary checks via VKGuard
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

        var chunks = VKWeavingStepRunner.ChunkSteps(
            _tasks,
            t => t.TaskOrder,
            t => t.ParallelGroup);

        bool hasFailed = false;
        VKResult? failedResult = null;

        var runResult = await VKWeavingStepRunner.ExecuteChunksAsync(
            chunks,
            context,
            t => t.IsParallel,
            (t, ctx, ct) => t.ExecuteAsync(ctx, ct),
            ctx => !hasFailed,
            (ctx, res) =>
            {
                hasFailed = true;
                failedResult = res;
            },
            cancellationToken).ConfigureAwait(false); // // [CS.03]

        if (hasFailed && failedResult is not null)
        {
            return VKResult.Failure<VKPsycheResponse>(failedResult.Errors); // // [CS.01]
        }

        if (context.Response.Messages.Count == 0)
        {
            return VKResult.Failure<VKPsycheResponse>(VKWeavingErrors.NoTapestry);
        }

        return VKResult.Success(context.Response.Build());
    }
}
