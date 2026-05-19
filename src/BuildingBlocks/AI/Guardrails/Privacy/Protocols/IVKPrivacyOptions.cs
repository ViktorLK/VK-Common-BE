using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Aggregates all Privacy Guard configuration settings.
/// </summary>
public interface IVKPrivacyOptions :
    IVKAIProviderOptions,
    IVKAIGovernanceOptions,
    IVKToggleableBlockOptions
{
}
