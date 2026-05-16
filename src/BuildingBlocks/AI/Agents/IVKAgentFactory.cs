using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Factory for creating agent instances.
/// Following AP.03 (Level 1 Public).
/// </summary>
public interface IVKAgentFactory
{
    /// <summary>
    /// Creates an agent instance.
    /// The specific implementation (Simple vs Cognitive) depends on registered providers.
    /// </summary>
    /// <param name="name">The name of the agent.</param>
    /// <param name="description">The description of the agent.</param>
    /// <param name="tools">The tools available to the agent.</param>
    /// <param name="metadata">The metadata for the agent.</param>
    /// <returns>A new <see cref="IVKAgent"/> instance.</returns>
    IVKAgent CreateAgent(
        string name,
        string description,
        IEnumerable<IVKAtomicTool> tools,
        IReadOnlyDictionary<string, object>? metadata = null);
}
