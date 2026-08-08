using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressSensors.Internal;

internal sealed class IngressSensorsPipelineStage : IVKPsychePipelineStage
{
    private readonly IVKSystemEventDispatcher _eventDispatcher;
    private readonly VKIngressSensorsOptions _options;
    private readonly ILogger<IngressSensorsPipelineStage> _logger;

    public bool IsActive => _options.Enabled;

    public VKPipelineSchedule Schedule => new(250, false, null, VKPipelinePhase.Before);

    public IngressSensorsPipelineStage(
        IVKSystemEventDispatcher eventDispatcher,
        IOptionsSnapshot<VKIngressSensorsOptions> options,
        ILogger<IngressSensorsPipelineStage> logger)
    {
        _eventDispatcher = VKGuard.NotNull(eventDispatcher);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        var result = await _eventDispatcher.ConsumeEventsAsync(cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to consume system events: {Error}", result.FirstError);
            return VKResult.Success();
        }

        if (result.Value.Count > 0)
        {
            context.SetState<IReadOnlyList<VKSystemEvent>>(result.Value);
        }

        return VKResult.Success();
    }
}
