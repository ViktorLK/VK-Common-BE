using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

internal sealed class DefaultWeavingPipelineStage : IVKOrchestrationPipelineStage
{
    private readonly VKWeavingOptions _options;
    private readonly IVKPromptWeavingEngine _weavingEngine;

    public int Order => 600;

    public bool IsActive => true;

    public bool IsParallel => false;

    public int? ParallelGroup => null;

    public DefaultWeavingPipelineStage(
        IOptions<VKWeavingOptions> options,
        IVKPromptWeavingEngine weavingEngine)
    {
        _options = VKGuard.NotNull(options).Value;
        _weavingEngine = VKGuard.NotNull(weavingEngine);
    }

    public async Task ExecuteAsync(VKOrchestrationPipelineContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);
        
        var tapestryResult = await _weavingEngine.WeavePromptAsync(context, ct).ConfigureAwait(false);
        if (tapestryResult.IsFailure)
        {
            context.CriticalError = VKPipelineError.From<DefaultWeavingPipelineStage>(tapestryResult.FirstError.Description);
            return;
        }
        
        context.Tapestry = tapestryResult.Value;
    }
}
