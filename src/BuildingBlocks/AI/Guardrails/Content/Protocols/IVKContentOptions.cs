using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Aggregates all Content Guard configuration settings.
/// </summary>
public interface IVKContentOptions :
    IVKAIProviderOptions,
    IVKAIGovernanceOptions,
    IVKToggleableBlockOptions
{
}
