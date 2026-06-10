using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Guardrails.Internal;

/// <summary>
/// Placeholder stage for Multi-modal Vision input preprocessing and safety check (OCR, Image moderation).
/// Complies with AP.01, AP.03, CS.01, and CS.03.
/// </summary>
internal sealed class AfferentVisionPipelineStage : IVKPsychePipelineStage
{
    private readonly ILogger<AfferentVisionPipelineStage> _logger;

    public int StageOrder => 150; // Executes after text Guardrails, before Text normalization

    public bool IsActive => true;

    public bool IsParallel => false;

    public int? ParallelGroup => null;

    public AfferentVisionPipelineStage(ILogger<AfferentVisionPipelineStage> logger)
    {
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        // // TODO: Retrieve image attachments from context extensions
        // // TODO: Implement OCR / safety scans for visual assets

        _logger.LogDebug("AfferentVisionPipelineStage placeholder execution.");
        return Task.FromResult(VKResult.Success());
    }
}
