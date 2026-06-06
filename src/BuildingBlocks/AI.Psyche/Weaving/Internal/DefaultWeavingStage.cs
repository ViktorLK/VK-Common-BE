using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed class DefaultWeavingStage : IVKWeavingStage
{
    private readonly IVKWeavingTaskEngine _weavingEngine;

    public DefaultWeavingStage(IVKWeavingTaskEngine weavingEngine)
    {
        _weavingEngine = VKGuard.NotNull(weavingEngine);
    }

    public int StageOrder => VKWeavingStageOrder.Weaving;
    public bool IsActive => true;
    public bool IsParallel => false;
    public int? ParallelGroup => null;

    public async Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var weaveResult = await _weavingEngine.WeavePromptAsync(context, cancellationToken).ConfigureAwait(false);
        if (weaveResult.IsFailure)
        {
            return VKResult.Failure(weaveResult.Errors);
        }

        return VKResult.Success();
    }
}
