using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche.Weaving.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

[VKTrace("psyche.stage.weaving")]
internal sealed class DefaultWeavingStage : IVKPsychePipelineStage
{
    private readonly IEnumerable<IVKWeavingPipelineTask> _tasks;
    private readonly ILogger<DefaultWeavingStage> _logger;

    public DefaultWeavingStage(
        IEnumerable<IVKWeavingPipelineTask> tasks,
        ILogger<DefaultWeavingStage> logger)
    {
        _tasks = VKGuard.NotNull(tasks);
        _logger = VKGuard.NotNull(logger);
    }

    public VKPipelineSchedule Schedule => VKPsychePipelineScheduler.Before.PsycheWeaving;

    public bool IsActive => true;

    public IEnumerable<IVKStageChild<VKPsycheContext>> Children => _tasks;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context); // [AP.01]

        if (context.IsAborted)
        {
            return VKResult.Failure(VKPipelineErrors.Aborted); // [CS.01]
        }

        var stopwatch = Stopwatch.StartNew();

        var runResult = await VKPipelineRunner.ExecuteComponentsAsync(
            _tasks,
            context,
            checkAbortedFunc: static ctx => ctx.IsAborted,
            abortResultFunc: static _ => VKResult.Failure(VKPipelineErrors.Aborted),
            checkCompletedFunc: static ctx => ctx.IsCompleted,
            cancellationToken: cancellationToken).ConfigureAwait(false); // [CS.03]

        if (runResult.IsFailure)
        {
            WeavingDiagnostics.RecordWeaving(stopwatch.Elapsed.TotalMilliseconds, "Weaving", false);
            return runResult; // [CS.01]
        }

        if (context.ResponseBuilder.Messages.Count == 0)
        {
            _logger.WeavingEmptyActive(context.Request.SessionId);
            WeavingDiagnostics.RecordWeaving(stopwatch.Elapsed.TotalMilliseconds, "Weaving", false);
            return VKResult.Failure(VKWeavingErrors.NoTapestry); // [CS.01]
        }

        var messageCount = context.ResponseBuilder.Messages.Count;
        _logger.WeavingAssembled(context.Request.SessionId, messageCount);

        var assembledTokens = context.ResponseBuilder.TotalEstimatedTokens;

        WeavingDiagnostics.RecordTokensAssembled(assembledTokens, "Weaving");
        WeavingDiagnostics.RecordWeaving(stopwatch.Elapsed.TotalMilliseconds, "Weaving", true);

        if (context.IsWeaveOnly)
        {
            context.Complete();
        }

        return VKResult.Success(); // [CS.01]
    }
}
