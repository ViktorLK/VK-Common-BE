using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent.EgressGuardrails.Internal;

internal sealed class EgressGuardrailsPipelineStage : IVKPsychePipelineStage
{
    private readonly IVKEgressGuardrail _outputGuardrail;
    private readonly VKEgressGuardrailsOptions _options;
    private readonly ILogger<EgressGuardrailsPipelineStage> _logger;

    public bool IsActive => _options.Enabled;

    public VKPipelineSchedule Schedule => new(100, false, null, VKPipelinePhase.After);

    public EgressGuardrailsPipelineStage(
        IVKEgressGuardrail outputGuardrail,
        IOptionsSnapshot<VKEgressGuardrailsOptions> options,
        ILogger<EgressGuardrailsPipelineStage> logger)
    {
        _outputGuardrail = VKGuard.NotNull(outputGuardrail);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (context.ResponseBuilder.ChatResponse?.Message is null)
        {
            return VKResult.Success();
        }

        var rawContent = context.ResponseBuilder.ChatResponse.Message.Content;
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return VKResult.Success();
        }

        var safetyResult = await _outputGuardrail.ValidateOutputSafetyAsync(rawContent, cancellationToken).ConfigureAwait(false);
        if (safetyResult.IsFailure)
        {
            return VKResult.Failure(safetyResult.FirstError);
        }

        if (safetyResult.Value != rawContent)
        {
            _logger.LogInformation("Egress guardrail sanitized/modified the response content.");
            var originalMsg = context.ResponseBuilder.ChatResponse.Message;
            var updatedMsg = originalMsg with { Content = safetyResult.Value };
            context.ResponseBuilder.ChatResponse = context.ResponseBuilder.ChatResponse with { Message = updatedMsg };
        }

        return VKResult.Success();
    }
}
