using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent;

/// <summary>
/// Defines the public contract interface for Afferent Guardrails configuration options.
/// Follows AP.01, AP.03.
/// </summary>
public interface IVKAfferentGuardrailsOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether content moderation is enabled.
    /// </summary>
    bool EnableContentModeration { get; }

    /// <summary>
    /// Gets a value indicating whether injection detection is enabled.
    /// </summary>
    bool EnableInjectionDetection { get; }

    /// <summary>
    /// Gets a value indicating whether privacy filtering is enabled.
    /// </summary>
    bool EnablePrivacyFiltering { get; }

    /// <summary>
    /// Gets a value indicating whether to block the pipeline on a violation.
    /// </summary>
    bool BlockOnViolation { get; }
}
