using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Internal implementation of <see cref="IVKPresenceStressMonitor"/>.
/// Follows AP.01 (Sealed Class), AP.03 (No VK prefix), and CS.06 (Deterministic clock).
/// </summary>
internal sealed class PresenceStressMonitor : IVKPresenceStressMonitor
{
    private readonly TimeProvider _timeProvider;
    private double _currentStress = 0.0;
    private DateTimeOffset _lastUpdate;
    private readonly double _decayRate = 0.05; // Stress decays by 5% per minute

    public PresenceStressMonitor(TimeProvider timeProvider)
    {
        _timeProvider = VKGuard.NotNull(timeProvider); // [AP.01] Boundary check
        _lastUpdate = _timeProvider.GetUtcNow();
    }

    public double CurrentStress
    {
        get
        {
            ApplyDecay();
            return _currentStress;
        }
    }

    public VKResult<VKPresenceStressEffect> ProcessSignal(IVKPresenceSignal signal)
    {
        VKGuard.NotNull(signal); // [AP.01] Boundary check

        if (signal.Intensity < 0.0 || signal.Intensity > 1.0)
        {
            return VKResult.Failure<VKPresenceStressEffect>(
                VKError.Validation(PresenceErrors.Signal.OutOfBounds, "Signal Intensity must be between 0.0 and 1.0."));
        }

        ApplyDecay();

        // 2 用 calmingSignal: Positive polarity/valence reduces stress immediately
        double stressDelta;
        if (signal.Polarity < 0.0)
        {
            // Negative valence/polarity increases stress
            stressDelta = signal.Intensity * (1.0 - signal.Polarity) * signal.Weight;
        }
        else
        {
            // Positive valence/polarity (calming signal) reduces stress
            stressDelta = -signal.Intensity * signal.Polarity * signal.Weight;
        }

        _currentStress = Math.Clamp(_currentStress + stressDelta, 0.0, 1.0);
        _lastUpdate = _timeProvider.GetUtcNow();

        var effect = _currentStress switch
        {
            > 0.9 => VKPresenceStressEffect.Shutdown,
            > 0.7 => VKPresenceStressEffect.Panic,
            > 0.5 => VKPresenceStressEffect.Alert,
            > 0.2 => VKPresenceStressEffect.Mild,
            _ => VKPresenceStressEffect.None
        };

        return VKResult.Success(effect);
    }

    private void ApplyDecay()
    {
        var now = _timeProvider.GetUtcNow();
        var minutesPassed = (now - _lastUpdate).TotalMinutes;

        if (minutesPassed > 0.0)
        {
            _currentStress *= Math.Pow(1.0 - _decayRate, minutesPassed);
            _lastUpdate = now;
        }
    }
}
