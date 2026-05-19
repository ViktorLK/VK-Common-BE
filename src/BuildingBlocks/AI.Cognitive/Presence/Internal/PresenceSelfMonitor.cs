using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Internal implementation of <see cref="IVKPresenceSelfMonitor"/>.
/// Evaluates turn sentiment and evolves persona traits dynamically.
/// Follows AP.01 (Sealed Class), AP.03 (No VK prefix), and CS.03 (ConfigureAwait).
/// </summary>
internal sealed class PresenceSelfMonitor : IVKPresenceSelfMonitor
{
    private readonly IVKPersonaCodex _personaCodex;
    private readonly IVKPresenceStressMonitor _stressMonitor;
    private readonly ILogger<PresenceSelfMonitor> _logger;

    public PresenceSelfMonitor(
        IVKPersonaCodex personaCodex,
        IVKPresenceStressMonitor stressMonitor,
        ILogger<PresenceSelfMonitor> logger)
    {
        _personaCodex = VKGuard.NotNull(personaCodex); // [AP.01] Boundary check
        _stressMonitor = VKGuard.NotNull(stressMonitor);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult> EvaluateTurnAsync(
        VKPresenceTurnContext turnContext,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(turnContext); // [AP.01] Boundary check

        try
        {
            // 1. Evaluate User Sentiment vs AI Effort to evolve traits
            // Happy path: update 'Affinity' positively
            if (turnContext.UserSentiment > 0.6)
            {
                await UpdateTraitAsync(turnContext.PersonaId, "Affinity", 0.1, cancellationToken).ConfigureAwait(false); // [CS.03]
            }
            // Conflict path: update 'Affinity' negatively
            else if (turnContext.UserSentiment < -0.6)
            {
                await UpdateTraitAsync(turnContext.PersonaId, "Affinity", -0.2, cancellationToken).ConfigureAwait(false); // [CS.03]
            }

            // 2 用 calmingSignal: If sentiment is positive, feed calming signal to flat decay stress
            if (turnContext.UserSentiment > 0.4)
            {
                var calmingSignal = new VKPresenceSignal
                {
                    Intensity = 0.5,
                    Polarity = 1.0, // Fully positive polarity
                    Valence = 0.5,
                    Weight = 0.3
                };
                _stressMonitor.ProcessSignal(calmingSignal);
            }

            return VKResult.Success();
        }
        catch (Exception ex)
        {
            // [OR.01] In a complete implementation we use LoggerMessage. For now standard logging is mapped inside Catch blocks
            _logger.LogError(ex, "Error occurred during turn self-monitoring for session {SessionId}.", turnContext.SessionId);
            return VKResult.Failure(VKError.Failure(PresenceErrors.SelfMonitor.EvaluationFailed, "Failed to evaluate presence turn."));
        }
    }

    private async Task UpdateTraitAsync(string personaId, string traitKey, double delta, CancellationToken ct)
    {
        var personaResult = await _personaCodex.GetPersonaAsync(personaId, ct).ConfigureAwait(false); // [CS.03]
        if (!personaResult.IsSuccess)
        {
            return;
        }

        var persona = personaResult.Value;
        var traits = new Dictionary<string, string>(persona.Traits);

        double currentVal = 0.5;
        if (traits.TryGetValue(traitKey, out var valStr) && double.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
        {
            currentVal = val;
        }

        double newVal = Math.Clamp(currentVal + delta, 0.0, 1.0);
        traits[traitKey] = newVal.ToString("F2", CultureInfo.InvariantCulture);

        var updatedPersona = persona with { Traits = traits };
        await _personaCodex.UpdatePersonaAsync(persona.Id, updatedPersona, ct).ConfigureAwait(false); // [CS.03]
    }
}
