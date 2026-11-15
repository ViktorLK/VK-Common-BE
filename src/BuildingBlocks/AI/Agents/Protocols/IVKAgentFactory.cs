using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Factory for creating agent instances, cooperative groups, and agent-as-tool wrappers.
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
    /// <param name="instructions">The instructions (system prompt) for the agent.</param>
    /// <param name="tools">The tools available to the agent.</param>
    /// <param name="metadata">The metadata for the agent.</param>
    /// <returns>A new <see cref="IVKAgent"/> instance.</returns>
    IVKAgent CreateAgent(
        string name,
        string description,
        string instructions = "",
        IEnumerable<IVKAtomicTool>? tools = null,
        IReadOnlyDictionary<string, object>? metadata = null);

    /// <summary>
    /// Creates a runnable agent instance from a declarative <see cref="VKAgentDefinition"/>.
    /// </summary>
    /// <param name="definition">The declarative agent definition.</param>
    /// <returns>A new <see cref="IVKAgent"/> instance.</returns>
    IVKAgent CreateAgent(VKAgentDefinition definition)
    {
        VKGuard.NotNull(definition);

        Dictionary<string, object>? metadata = definition.Metadata is not null
            ? new Dictionary<string, object>(definition.Metadata)
            : null;

        if (!string.IsNullOrWhiteSpace(definition.ModelProfile))
        {
            metadata ??= new Dictionary<string, object>();
            metadata.TryAdd("client_name", definition.ModelProfile);
        }

        return CreateAgent(
            definition.Name,
            definition.Description,
            definition.Instructions,
            definition.Tools,
            metadata);
    }

    /// <summary>
    /// Creates a cooperative agent group instance.
    /// </summary>
    /// <returns>A new <see cref="IVKAgentGroup"/> instance.</returns>
    IVKAgentGroup CreateAgentGroup();

    /// <summary>
    /// Adapts an <see cref="IVKAgent"/> into an <see cref="IVKAtomicTool"/> for hierarchical sub-agent composition.
    /// </summary>
    /// <param name="agent">The child agent to expose as a callable tool.</param>
    /// <returns>An <see cref="IVKAtomicTool"/> delegating invocation to <paramref name="agent"/>.</returns>
    IVKAtomicTool AsTool(IVKAgent agent) => new Agents.Internal.AgentAtomicToolAdapter(agent);
}
