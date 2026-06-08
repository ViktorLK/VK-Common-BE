using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.AI.Afferent;
using VK.Blocks.AI.Afferent.Guardrails.Diagnostics.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Guardrails.Internal;

internal sealed class AfferentGuardrailsPipelineStage : IVKPsychePipelineStage
{
    private readonly IVKGuardrail _guardrail;
    private readonly VKAfferentGuardrailsOptions _options;
    private readonly ILogger<AfferentGuardrailsPipelineStage> _logger;

    public int StageOrder => 100; // Executes first for safety

    public bool IsActive => _options.Enabled;

    public bool IsParallel => false;

    public int? ParallelGroup => null;

    public AfferentGuardrailsPipelineStage(
        IVKGuardrail guardrail,
        IOptionsSnapshot<VKAfferentGuardrailsOptions> options,
        ILogger<AfferentGuardrailsPipelineStage> logger)
    {
        _guardrail = VKGuard.NotNull(guardrail);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> ExecuteAsync(VKWeavingContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (string.IsNullOrWhiteSpace(context.UserInput))
        {
            return VKResult.Success();
        }

        var safetyResult = await _guardrail.ValidateSafetyAsync(context.UserInput, cancellationToken).ConfigureAwait(false);
        if (safetyResult.IsFailure)
        {
            return VKResult.Failure(safetyResult.FirstError);
        }

        // If the guardrail masked or modified the input (e.g. privacy masking), save it in extensions or log it
        if (safetyResult.Value != context.UserInput)
        {
            _logger.LogInformation("Guardrail modified user input (e.g. masked PII).");
            context.SetExtension<string>(safetyResult.Value);
        }

        return VKResult.Success();
    }
}
