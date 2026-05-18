using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Aggregates all Chat configuration settings.
/// Following Mode B: Strict Contractual Mapping.
/// </summary>
public interface IVKChatOptions :
    IVKAIProviderOptions,
    IVKAIGovernanceOptions,
    IVKGenerationOptions,
    IVKToggleableBlockOptions
{
    // Note: Any Chat-specific settings that are NOT overridable should be defined here,
    // and they should NOT be present in IVKChatOverrides.
}
