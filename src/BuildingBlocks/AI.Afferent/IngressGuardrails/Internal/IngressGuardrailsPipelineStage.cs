using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressGuardrails.Internal;

internal sealed class IngressGuardrailsPipelineStage : IVKPsycheBeforePipelineStage
{
    private readonly IVKIngressGuardrail _guardrail;
    private readonly VKIngressGuardrailsOptions _options;
    private readonly ILogger<IngressGuardrailsPipelineStage> _logger;

    public bool IsActive => _options.Enabled;

    public VKPipelineStageSchedule Schedule => new(100, false); // Executes first for safety

    public IngressGuardrailsPipelineStage(
        IVKIngressGuardrail guardrail,
        IOptionsSnapshot<VKIngressGuardrailsOptions> options,
        ILogger<IngressGuardrailsPipelineStage> logger)
    {
        _guardrail = VKGuard.NotNull(guardrail);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKPsycheContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (string.IsNullOrWhiteSpace(context.Request.UserInput))
        {
            return VKResult.Success();
        }

        var safetyResult = await _guardrail.ValidateSafetyAsync(context.Request.UserInput, cancellationToken).ConfigureAwait(false);
        if (safetyResult.IsFailure)
        {
            return VKResult.Failure(safetyResult.FirstError);
        }

        if (safetyResult.Value != context.Request.UserInput)
        {
            _logger.LogInformation("Ingress guardrail modified user input (e.g. masked PII).");
            context.SetState<string>(safetyResult.Value);
        }

        return VKResult.Success();
    }
}
