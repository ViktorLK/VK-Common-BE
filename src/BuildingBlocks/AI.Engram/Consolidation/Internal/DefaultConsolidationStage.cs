using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation.Internal;

/// <summary>
/// Pipeline stage for consolidating engrams.
/// </summary>
internal sealed class DefaultConsolidationStage : IVKPsycheAfterPipelineStage
{
    private readonly IVKConsolidationService _consolidationService;
    private readonly VKConsolidationOptions _options;

    public DefaultConsolidationStage(IVKConsolidationService consolidationService, IOptions<VKConsolidationOptions> options)
    {
        _consolidationService = VKGuard.NotNull(consolidationService);
        _options = VKGuard.NotNull(options?.Value);
    }

    public bool IsActive => _options.Enabled;

    public VKPipelineStageSchedule Schedule => new VKPipelineStageSchedule(100, false);

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        if (!IsActive)
        {
            return VKResult.Success();
        }

        return await _consolidationService.ConsolidateSessionMemoryAsync(context, cancellationToken).ConfigureAwait(false);
    }
}
