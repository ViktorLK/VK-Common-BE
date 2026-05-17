namespace VK.Blocks.AI;

/// <summary>
/// Defines privacy guard parameters that can be overridden at the request level.
/// </summary>
public interface IVKPrivacyOverrides :
    IVKAIProviderOverrides,
    IVKAIGovernanceOverrides
{
    /// <summary>
    /// Gets the list of PII types to detect.
    /// </summary>
    System.Collections.Generic.IReadOnlyList<string>? Categories { get; init; }

    /// <summary>
    /// Gets the masking character.
    /// </summary>
    char? MaskingChar { get; init; }
}
