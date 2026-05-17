namespace VK.Blocks.AI;

/// <summary>
/// Defines content guard parameters that can be overridden at the request level.
/// </summary>
public interface IVKContentOverrides :
    IVKAIProviderOverrides,
    IVKAIGovernanceOverrides
{
    /// <summary>
    /// Gets a value indicating whether to automatically block requests that fail moderation.
    /// </summary>
    bool? AutoBlockOnFailure { get; init; }

    /// <summary>
    /// Gets the sensitivity threshold.
    /// </summary>
    float? SensitivityThreshold { get; init; }
}
