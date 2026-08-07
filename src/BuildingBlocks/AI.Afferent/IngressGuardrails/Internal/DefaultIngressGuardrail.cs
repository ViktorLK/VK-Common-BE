using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressGuardrails.Internal;

internal sealed class DefaultIngressGuardrail : IVKIngressGuardrail
{
    private readonly IVKPrivacyFilter _privacyFilter;
    private readonly IVKInjectionDetector _injectionDetector;
    private readonly IVKModerationEngine _moderationEngine;
    private readonly VKIngressGuardrailsOptions _options;
    private readonly ILogger<DefaultIngressGuardrail> _logger;

    public DefaultIngressGuardrail(
        IVKPrivacyFilter privacyFilter,
        IVKInjectionDetector injectionDetector,
        IVKModerationEngine moderationEngine,
        IOptionsSnapshot<VKIngressGuardrailsOptions> options,
        ILogger<DefaultIngressGuardrail> logger)
    {
        _privacyFilter = VKGuard.NotNull(privacyFilter);
        _injectionDetector = VKGuard.NotNull(injectionDetector);
        _moderationEngine = VKGuard.NotNull(moderationEngine);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<string>> ValidateSafetyAsync(string text, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(text);

        var processedText = text;

        // 1. Content Moderation
        if (_options.EnableContentModeration)
        {
            var modResult = await _moderationEngine.CheckContentAsync(processedText, cancellationToken).ConfigureAwait(false);
            if (modResult.IsFailure)
            {
                _logger.LogWarning("Content moderation check failed: {Error}", modResult.FirstError);
                if (_options.BlockOnViolation)
                {
                    return VKResult.Failure<string>(modResult.FirstError);
                }
            }
            else if (modResult.Value.IsFlagged)
            {
                _logger.LogWarning("Content violation detected: {Reason}", modResult.Value.FlaggedReason);
                if (_options.BlockOnViolation)
                {
                    return VKResult.Failure<string>(VKError.Validation("Afferent.Guardrails.Violation", $"Content policy violation: {modResult.Value.FlaggedReason}"));
                }
            }
        }

        // 2. Prompt Injection Detection
        if (_options.EnableInjectionDetection)
        {
            var injectResult = await _injectionDetector.DetectAsync(processedText, cancellationToken).ConfigureAwait(false);
            if (injectResult.IsFailure)
            {
                _logger.LogWarning("Injection detection failed: {Error}", injectResult.FirstError);
            }
            else if (injectResult.Value.IsInjectionDetected)
            {
                _logger.LogWarning("Prompt injection attempt detected! Score: {Score}", injectResult.Value.ConfidenceScore);
                if (_options.BlockOnViolation)
                {
                    return VKResult.Failure<string>(VKError.Forbidden("Afferent.Guardrails.Injection", "Prompt injection attack detected."));
                }
            }
        }

        // 3. Privacy Filtering (PII Masking)
        if (_options.EnablePrivacyFiltering)
        {
            var piiResult = await _privacyFilter.MaskAsync(processedText, cancellationToken).ConfigureAwait(false);
            if (piiResult.IsFailure)
            {
                _logger.LogWarning("PII filtering failed: {Error}", piiResult.FirstError);
            }
            else
            {
                processedText = piiResult.Value.MaskedText;
            }
        }

        return VKResult.Success(processedText);
    }
}
