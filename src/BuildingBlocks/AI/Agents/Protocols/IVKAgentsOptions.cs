using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Aggregates all Agent configuration settings.
/// </summary>
public interface IVKAgentsOptions :
    IVKAIProviderOptions,
    IVKAIGovernanceOptions,
    IVKToggleableBlockOptions
{
}
