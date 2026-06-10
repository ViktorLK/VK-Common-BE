using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Configuration options for the Afferent Guardrails feature.
/// Follows AP.01, AP.03, and BB.07.
/// </summary>
[VKFeature(typeof(VKAIAfferentBlock), GenerateValidator = true, Namespace = "VK.Blocks.AI.Afferent.Guardrails")]
public sealed partial record VKAfferentGuardrailsOptions : IVKAfferentGuardrailsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Afferent Guardrails is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether content moderation is enabled.
    /// Defaults to true.
    /// </summary>
    public bool EnableContentModeration { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether injection detection is enabled.
    /// Defaults to true.
    /// </summary>
    public bool EnableInjectionDetection { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether privacy filtering is enabled.
    /// Defaults to true.
    /// </summary>
    public bool EnablePrivacyFiltering { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to block the pipeline on a violation.
    /// Defaults to true.
    /// </summary>
    public bool BlockOnViolation { get; init; } = true;
}
