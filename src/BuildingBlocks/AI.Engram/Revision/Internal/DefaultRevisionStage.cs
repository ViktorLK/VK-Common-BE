using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Revision.Internal;

/// <summary>
/// Pipeline stage running AFTER the LLM execution to apply memory revision.
/// </summary>
internal sealed class DefaultRevisionStage : IVKPsychePipelineStage
{
    private readonly IVKRevisionService _revisionService;
    private readonly VKRevisionOptions _options;

    public DefaultRevisionStage(IVKRevisionService revisionService, IOptions<VKRevisionOptions> options)
    {
        _revisionService = VKGuard.NotNull(revisionService);
        _options = VKGuard.NotNull(options?.Value);
    }

    public bool IsActive => _options.Enabled;

    public VKPipelineSchedule Schedule => new(200, false, null, VKPipelinePhase.After);

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken)
    {
        VKGuard.NotNull(context);

        if (!IsActive)
        {
            return VKResult.Success();
        }

        return await _revisionService.ReviseSessionMemoriesAsync(context, cancellationToken).ConfigureAwait(false);
    }
}
