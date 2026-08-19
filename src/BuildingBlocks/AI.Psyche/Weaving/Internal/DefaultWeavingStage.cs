using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed class DefaultWeavingStage : IVKPsychePipelineStage
{
    private readonly IEnumerable<IVKWeavingPipelineTask> _tasks;
    private readonly VKWeavingOptions _options;

    public DefaultWeavingStage(
        IEnumerable<IVKWeavingPipelineTask> tasks,
        VKWeavingOptions options)
    {
        _tasks = VKGuard.NotNull(tasks);
        _options = VKGuard.NotNull(options);
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheWeaving;

    public bool IsActive => true;

    public IEnumerable<IVKStageChild<VKPsycheContext>> Children => _tasks;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context); // [AP.01]

        var stopwatch = Stopwatch.StartNew();
        try
        {
            PruneDisabledTiers(context);

            var runResult = await VKPipelineRunner.ExecuteComponentsAsync(_tasks, context, cancellationToken: cancellationToken).ConfigureAwait(false); // [CS.03]
            if (runResult.IsFailure)
            {
                return runResult; // [CS.01]
            }

            if (context.ResponseBuilder.Messages.Count == 0)
            {
                return VKResult.Failure(VKWeavingErrors.NoTapestry); // [CS.01]
            }

            if (context.IsWeaveOnly)
            {
                context.Complete();
            }

            return VKResult.Success(); // [CS.01]
        }
        finally
        {
            stopwatch.Stop();
            context.ResponseBuilder.ProfilingMetrics[VKPsycheProfilingKeys.WeavingStage] = stopwatch.Elapsed.TotalMilliseconds;
        }
    }

    private void PruneDisabledTiers(VKPsycheContext context)
    {
        var disabledTiers = context.Args<VKWeavingArgs>()?.DisabledTiers ?? _options.DisabledTiers;
        if (disabledTiers is not null && disabledTiers.Count > 0)
        {
            var activeFragments = context.Fragments
                .Where(f => !disabledTiers.Contains(f.TierType))
                .ToList();
            context.SetFragments(activeFragments);
        }
    }
}
