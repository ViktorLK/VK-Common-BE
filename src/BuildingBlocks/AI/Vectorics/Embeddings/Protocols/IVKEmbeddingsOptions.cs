using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Aggregates all Embeddings configuration settings.
/// </summary>
public interface IVKEmbeddingsOptions :
    IVKAIProviderOptions,
    IVKAIGovernanceOptions,
    IVKToggleableBlockOptions
{
}
