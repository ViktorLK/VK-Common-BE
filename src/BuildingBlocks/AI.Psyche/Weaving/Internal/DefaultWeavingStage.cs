using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VK.Blocks.AI.Psyche.Weaving.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

[VKTrace("psyche.stage.weaving")]
internal sealed class DefaultWeavingStage : IVKPsychePipelineStage
{
    private readonly IEnumerable<IVKWeavingPipelineTask> _tasks;
    private readonly VKWeavingOptions _options;
    private readonly ILogger<DefaultWeavingStage> _logger;

    public DefaultWeavingStage(
        IEnumerable<IVKWeavingPipelineTask> tasks,
        VKWeavingOptions options,
        ILogger<DefaultWeavingStage>? logger = null)
    {
        _tasks = VKGuard.NotNull(tasks);
        _options = VKGuard.NotNull(options);
        _logger = logger ?? NullLogger<DefaultWeavingStage>.Instance;
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheWeaving;

    public bool IsActive => true;

    public IEnumerable<IVKStageChild<VKPsycheContext>> Children => _tasks;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context); // [AP.01]

        PruneDisabledTiers(context);

        var runResult = await VKPipelineRunner.ExecuteComponentsAsync(_tasks, context, cancellationToken: cancellationToken).ConfigureAwait(false); // [CS.03]
        if (runResult.IsFailure)
        {
            return runResult; // [CS.01]
        }

        if (context.ResponseBuilder.Messages.Count == 0)
        {
            _logger.WeavingEmptyActive(context.Request.SessionId);
            return VKResult.Failure(VKWeavingErrors.NoTapestry); // [CS.01]
        }

        var messageCount = context.ResponseBuilder.Messages.Count;
        _logger.WeavingAssembled(context.Request.SessionId, messageCount);

        if (messageCount > 0)
        {
            WeavingDiagnostics.RecordTokensAssembled(messageCount, "Weaving");
        }

        if (context.IsWeaveOnly)
        {
            context.Complete();
        }

        return VKResult.Success(); // [CS.01]
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
