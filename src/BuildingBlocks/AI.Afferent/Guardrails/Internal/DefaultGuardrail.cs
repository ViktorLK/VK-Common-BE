using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Guardrails.Internal;

/// <summary>
/// Production-grade implementation of <see cref="IVKGuardrail"/>.
/// Complies with AP.01, AP.03, CS.01, and CS.03.
/// </summary>
internal sealed class DefaultGuardrail : IVKGuardrail
{
    private readonly IVKPrivacyFilter _privacyFilter;
    private readonly IVKInjectionDetector _injectionDetector;
    private readonly IVKModerationEngine _moderationEngine;
    private readonly VKAfferentGuardrailsOptions _options;
    private readonly ILogger<DefaultGuardrail> _logger;

    public DefaultGuardrail(
        IVKPrivacyFilter privacyFilter,
        IVKInjectionDetector injectionDetector,
        IVKModerationEngine moderationEngine,
        IOptionsSnapshot<VKAfferentGuardrailsOptions> options,
        ILogger<DefaultGuardrail> logger)
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

        var currentText = text;

        // 1. Content Moderation
        if (_options.EnableContentModeration)
        {
            var modResult = await _moderationEngine.CheckContentAsync(currentText, cancellationToken).ConfigureAwait(false);
            if (modResult.IsFailure)
            {
                return VKResult.Failure<string>(modResult.FirstError);
            }

            if (modResult.Value.IsFlagged)
            {
                _logger.LogWarning("Content flagged by moderation: {Reason}", modResult.Value.FlaggedReason);
                if (_options.BlockOnViolation)
                {
                    return VKResult.Failure<string>(GuardrailsErrors.ContentFlagged);
                }
            }
        }

        // 2. Injection Detection
        if (_options.EnableInjectionDetection)
        {
            var injectionResult = await _injectionDetector.DetectAsync(currentText, cancellationToken).ConfigureAwait(false);
            if (injectionResult.IsFailure)
            {
                return VKResult.Failure<string>(injectionResult.FirstError);
            }

            if (injectionResult.Value.IsInjectionDetected)
            {
                _logger.LogWarning("Prompt injection detected with confidence score {Score}.", injectionResult.Value.ConfidenceScore);
                if (_options.BlockOnViolation)
                {
                    return VKResult.Failure<string>(GuardrailsErrors.InjectionDetected);
                }
            }
        }

        // 3. Privacy Filtering
        if (_options.EnablePrivacyFiltering)
        {
            var privacyResult = await _privacyFilter.MaskAsync(currentText, cancellationToken).ConfigureAwait(false);
            if (privacyResult.IsFailure)
            {
                return VKResult.Failure<string>(privacyResult.FirstError);
            }

            // Update with masked text
            currentText = privacyResult.Value.MaskedText;
        }

        return VKResult.Success(currentText);
    }
}
