using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Efferent.EgressGuardrails.Internal;

internal sealed class DefaultEgressGuardrail : IVKEgressGuardrail
{
    private readonly IVKPrivacyFilter _privacyFilter;
    private readonly IVKModerationEngine _moderationEngine;
    private readonly VKEgressGuardrailsOptions _options;
    private readonly ILogger<DefaultEgressGuardrail> _logger;

    public DefaultEgressGuardrail(
        IVKPrivacyFilter privacyFilter,
        IVKModerationEngine moderationEngine,
        IOptionsSnapshot<VKEgressGuardrailsOptions> options,
        ILogger<DefaultEgressGuardrail> logger)
    {
        _privacyFilter = VKGuard.NotNull(privacyFilter);
        _moderationEngine = VKGuard.NotNull(moderationEngine);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<string>> ValidateOutputSafetyAsync(string text, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(text);

        var processedText = text;

        if (_options.EnableContentModeration)
        {
            var modResult = await _moderationEngine.CheckContentAsync(processedText, cancellationToken).ConfigureAwait(false);
            if (modResult.IsFailure)
            {
                _logger.LogWarning("Output content moderation check failed: {Error}", modResult.FirstError);
                if (_options.BlockOnViolation)
                {
                    return VKResult.Failure<string>(modResult.FirstError);
                }
            }
            else if (modResult.Value.IsFlagged)
            {
                _logger.LogWarning("Output content violation detected: {Reason}", modResult.Value.FlaggedReason);
                if (_options.BlockOnViolation)
                {
                    return VKResult.Failure<string>(VKError.Validation("Efferent.Guardrails.Violation", $"Output policy violation: {modResult.Value.FlaggedReason}"));
                }
            }
        }

        if (_options.EnableDataLeakPrevention)
        {
            var piiResult = await _privacyFilter.MaskAsync(processedText, cancellationToken).ConfigureAwait(false);
            if (piiResult.IsFailure)
            {
                _logger.LogWarning("Output PII filtering failed: {Error}", piiResult.FirstError);
            }
            else
            {
                processedText = piiResult.Value.MaskedText;
            }
        }

        return VKResult.Success(processedText);
    }
}
