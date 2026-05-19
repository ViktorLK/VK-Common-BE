using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Monitors environmental stress signals and maintains an accumulated stress level.
/// Follows AP.03 public contract rules.
/// </summary>
public interface IVKPresenceStressMonitor
{
    /// <summary>
    /// Gets the current accumulated stress level (0.0 to 1.0).
    /// </summary>
    double CurrentStress { get; }

    /// <summary>
    /// Processes an incoming presence signal to modify the stress state.
    /// Supports both negative stressors and positive calming reductions.
    /// </summary>
    /// <param name="signal">The environmental or biological signal.</param>
    /// <returns>A result containing the active stress effect state.</returns>
    VKResult<VKPresenceStressEffect> ProcessSignal(IVKPresenceSignal signal);
}

/// <summary>
/// Categorized thresholds of stress impacts.
/// </summary>
public enum VKPresenceStressEffect
{
    /// <summary>
    /// No stress effects.
    /// </summary>
    None,

    /// <summary>
    /// Mild stress, slight behavioral adjustments.
    /// </summary>
    Mild,

    /// <summary>
    /// Elevated stress, proactive alert state.
    /// </summary>
    Alert,

    /// <summary>
    /// High stress, panic thresholds.
    /// </summary>
    Panic,

    /// <summary>
    /// Critical stress, system shutdown / reactive state.
    /// </summary>
    Shutdown
}
