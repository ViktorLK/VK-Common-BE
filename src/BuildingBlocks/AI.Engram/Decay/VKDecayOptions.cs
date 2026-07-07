using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Options for the Decay stage.
/// </summary>
[VKFeature(typeof(VKAIEngramBlock))]
public sealed partial record VKDecayOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Decay stage is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the decay rate.
    /// </summary>
    public double DecayRate { get; init; } = 0.1;

    /// <summary>
    /// Gets or sets the time mode used for decay calculations.
    /// </summary>
    public VKDecayTimeMode TimeMode { get; init; } = VKDecayTimeMode.Hybrid;

    /// <summary>
    /// Gets or sets the stability baseline in days for L2 memories.
    /// </summary>
    public double L2StabilityBaseline { get; init; } = 7.0;

    /// <summary>
    /// Gets or sets the stability baseline in days for L3 memories.
    /// </summary>
    public double L3StabilityBaseline { get; init; } = 30.0;

    /// <summary>
    /// Gets or sets the multiplier applied to stability for explicit statement confidence.
    /// </summary>
    public double ExplicitConfidenceBonus { get; init; } = 1.5;

    /// <summary>
    /// Gets or sets the multiplier applied to stability for inferred statement confidence.
    /// </summary>
    public double InferredConfidencePenalty { get; init; } = 0.5;

    /// <summary>
    /// Gets or sets the multiplier applied to stability when emotional tagging intensity is high.
    /// </summary>
    public double EmotionalIntensityMultiplier { get; init; } = 1.2;

    /// <summary>
    /// Gets or sets the increment added to stability per memory access count.
    /// </summary>
    public double AccessCountMultiplier { get; init; } = 0.2;

    /// <summary>
    /// Gets or sets the interval in minutes between background decay batch jobs.
    /// </summary>
    public int DecayIntervalMinutes { get; init; } = 60;

    /// <summary>
    /// Gets or sets the weight applied to negative emotions when calculating base importance.
    /// </summary>
    public double NegativeEmotionWeight { get; init; } = 0.4;

    /// <summary>
    /// Gets or sets the weight applied to positive emotions when calculating base importance.
    /// </summary>
    public double PositiveEmotionWeight { get; init; } = 0.2;

    /// <summary>
    /// Gets or sets the multiplier applied to memory stability based on emotional arousal.
    /// </summary>
    public double EmotionalStabilityMultiplier { get; init; } = 0.5;
}
