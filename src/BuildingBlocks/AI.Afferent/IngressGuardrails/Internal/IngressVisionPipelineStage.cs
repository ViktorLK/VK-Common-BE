using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressGuardrails.Internal;

internal sealed class IngressVisionPipelineStage : IVKPsychePipelineStage
{
    private readonly ILogger<IngressVisionPipelineStage> _logger;

    public bool IsActive => true;

    public VKPipelineSchedule Schedule => new(150, false, null, VKPipelinePhase.Before); // Executes after text Guardrails, before IngressText

    public IngressVisionPipelineStage(ILogger<IngressVisionPipelineStage> logger)
    {
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        _logger.LogDebug("IngressVisionPipelineStage placeholder execution.");
        return Task.FromResult(VKResult.Success());
    }
}
