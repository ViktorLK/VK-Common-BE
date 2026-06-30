using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressText.Internal;

internal sealed class IngressDocumentPipelineStage : IVKPsycheBeforePipelineStage
{
    private readonly ILogger<IngressDocumentPipelineStage> _logger;

    public bool IsActive => true;

    public VKPipelineStageSchedule Schedule => new(350, false);

    public IngressDocumentPipelineStage(ILogger<IngressDocumentPipelineStage> logger)
    {
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        _logger.LogDebug("IngressDocumentPipelineStage placeholder execution.");
        return Task.FromResult(VKResult.Success());
    }
}
