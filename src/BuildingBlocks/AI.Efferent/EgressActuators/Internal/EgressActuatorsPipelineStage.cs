using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent.EgressActuators.Internal;

internal sealed class EgressActuatorsPipelineStage : IVKPsychePipelineStage
{
    private readonly IVKEgressActuators _actionDispatcher;
    private readonly VKEgressActuatorsOptions _options;
    private readonly ILogger<EgressActuatorsPipelineStage> _logger;

    public bool IsActive => _options.Enabled;

    public VKPipelineSchedule Schedule => new(300, false, null, VKPipelinePhase.After); // Executes after EgressText (200), before EgressAudio (500)

    public EgressActuatorsPipelineStage(
        IVKEgressActuators actionDispatcher,
        IOptionsSnapshot<VKEgressActuatorsOptions> options,
        ILogger<EgressActuatorsPipelineStage> logger)
    {
        _actionDispatcher = VKGuard.NotNull(actionDispatcher);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (context.Response.ChatResponse?.Message?.ToolCalls is null ||
            context.Response.ChatResponse.Message.ToolCalls.Count == 0)
        {
            return VKResult.Success();
        }

        var executionResult = await _actionDispatcher.DispatchActionsAsync(
            context.Response.ChatResponse.Message.ToolCalls,
            cancellationToken).ConfigureAwait(false);

        if (executionResult.IsFailure)
        {
            return VKResult.Failure(executionResult.FirstError);
        }

        context.SetState(executionResult.Value);

        return VKResult.Success();
    }
}
