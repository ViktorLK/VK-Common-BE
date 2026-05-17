namespace VK.Blocks.AI;

/// <summary>
/// Aggregates all Chat configuration settings.
/// Following Mode B: Strict Contractual Mapping.
/// </summary>
public interface IVKChatSettings :
    IVKAIProviderSettings,
    IVKAIGovernanceSettings,
    IVKGenerationSettings
{
    // Note: Any Chat-specific settings that are NOT overridable should be defined here,
    // and they should NOT be present in IVKChatOverrides.
}
