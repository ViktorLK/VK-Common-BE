using System.Collections.Generic;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Factory for creating advanced cognitive agent instances.
/// Following AP.03 (Level 1 Public).
/// </summary>
public interface IVKCognitiveAgentFactory
{
    /// <summary>
    /// Creates a cognitive agent with a specific persona.
    /// </summary>
    /// <param name="name">The name of the agent.</param>
    /// <param name="description">The description of the agent.</param>
    /// <param name="tools">The tools available to the agent.</param>
    /// <param name="personaId">The identifier of the persona to use.</param>
    /// <param name="metadata">The metadata for the agent.</param>
    /// <returns>A new <see cref="IVKAgent"/> instance.</returns>
    IVKAgent CreateAgent(
        string name,
        string description,
        IEnumerable<IVKAtomicTool> tools,
        string personaId,
        IReadOnlyDictionary<string, object>? metadata = null);

    /// <summary>
    /// Creates a cognitive agent that integrates with Persona and Memory.
    /// </summary>
    /// <param name="name">The name of the agent.</param>
    /// <param name="description">The description of the agent.</param>
    /// <param name="tools">The tools available to the agent.</param>
    /// <param name="metadata">The metadata for the agent.</param>
    IVKAgent CreateCognitiveAgent(
        string name,
        string description,
        IEnumerable<IVKAtomicTool> tools,
        IReadOnlyDictionary<string, object>? metadata = null);
}
