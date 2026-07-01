using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressTokenics.Internal;

internal sealed class IngressRateLimitPipelineStage : IVKPsycheBeforePipelineStage
{
    private readonly ILogger<IngressRateLimitPipelineStage> _logger;

    public bool IsActive => true;

    public VKPipelineStageSchedule Schedule => new(700, false);

    public IngressRateLimitPipelineStage(ILogger<IngressRateLimitPipelineStage> logger)
    {
        _logger = VKGuard.NotNull(logger);
    }

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);
        _logger.LogDebug("IngressRateLimitPipelineStage placeholder execution.");
        return Task.FromResult(VKResult.Success());
    }
}
