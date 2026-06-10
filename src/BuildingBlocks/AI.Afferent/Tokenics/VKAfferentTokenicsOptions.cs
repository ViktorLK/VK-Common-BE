using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Configuration options for the Afferent Tokenics feature.
/// Follows AP.01, AP.03, and BB.07.
/// </summary>
[VKFeature(typeof(VKAIAfferentBlock), GenerateValidator = true, Namespace = "VK.Blocks.AI.Afferent.Tokenics")]
public sealed partial record VKAfferentTokenicsOptions : IVKAfferentTokenicsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Afferent Tokenics is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum allowed input tokens.
    /// Defaults to 32768.
    /// </summary>
    public int MaxInputTokens { get; init; } = 32768;

    /// <summary>
    /// Gets or sets the warning threshold ratio (0.0 to 1.0) before triggering warnings.
    /// Defaults to 0.8f.
    /// </summary>
    public float BudgetWarningThreshold { get; init; } = 0.8f;

    /// <summary>
    /// Gets or sets a value indicating whether to strictly enforce the hard limit.
    /// Defaults to true.
    /// </summary>
    public bool EnforceHardLimit { get; init; } = true;
}
