using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Aggregates all Image Generation configuration settings.
/// </summary>
public interface IVKImageGenerationOptions :
    IVKAIProviderOptions,
    IVKAIGovernanceOptions,
    IVKToggleableBlockOptions
{
}
