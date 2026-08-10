using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Consolidation.Internal;

/// <summary>
/// Pipeline stage for enqueuing engrams for long-term consolidation.
/// </summary>
internal sealed class DefaultConsolidationStage : IVKPsychePipelineStage
{
    private readonly ConsolidationJobQueue _jobQueue;
    private readonly VKConsolidationOptions _options;

    public DefaultConsolidationStage(ConsolidationJobQueue jobQueue, IOptions<VKConsolidationOptions> options)
    {
        _jobQueue = VKGuard.NotNull(jobQueue);
        _options = VKGuard.NotNull(options?.Value);
    }

    public bool IsActive => _options.Enabled;

    public VKPipelineSchedule Schedule => new(100, false, null, VKPipelinePhase.After);

    public Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        var sessionId = context.Request.SessionId;
        if (!sessionId.IsEmpty)
        {
            _jobQueue.TryEnqueue(sessionId);
        }

        return Task.FromResult(VKResult.Success());
    }
}
