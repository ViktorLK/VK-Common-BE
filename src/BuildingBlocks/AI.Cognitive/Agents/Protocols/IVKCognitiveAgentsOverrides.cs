namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Request-level overrides contract for the Cognitive Agents feature.
/// </summary>
public interface IVKCognitiveAgentsOverrides :
    IVKAIProviderOverrides,
    IVKAIGovernanceOverrides
{
    /// <summary>
    /// Gets the overridden persona ID.
    /// </summary>
    string? DefaultPersonaId { get; init; }
}
