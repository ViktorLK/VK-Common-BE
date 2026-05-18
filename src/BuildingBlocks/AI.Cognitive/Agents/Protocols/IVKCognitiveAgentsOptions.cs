using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Options contract for the Cognitive Agents feature.
/// </summary>
public interface IVKCognitiveAgentsOptions :
    IVKAIProviderOptions,
    IVKAIGovernanceOptions,
    IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the default persona ID to use if not specified.
    /// </summary>
    string? DefaultPersonaId { get; }
}
