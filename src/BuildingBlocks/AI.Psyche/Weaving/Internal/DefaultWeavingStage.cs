using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed class DefaultWeavingStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKWeavingTaskEngine _weavingEngine;

    public DefaultWeavingStage(IVKWeavingTaskEngine weavingEngine)
    {
        _weavingEngine = VKGuard.NotNull(weavingEngine);
    }

    public int StageOrder => VKPsychePipelineScheduler.Before.Weaving.Order;
    public bool IsActive => true;
    public bool IsParallel => VKPsychePipelineScheduler.Before.Weaving.IsParallel;
    public int? ParallelGroup => VKPsychePipelineScheduler.Before.Weaving.ParallelGroup;

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context); // [AP.01]

        var weaveResult = await _weavingEngine.WeavePromptAsync(context, cancellationToken).ConfigureAwait(false); // [CS.03]
        if (weaveResult.IsFailure)
        {
            return VKResult.Failure(weaveResult.Errors); // [CS.01]
        }

        return VKResult.Success(); // [CS.01]
    }
}
