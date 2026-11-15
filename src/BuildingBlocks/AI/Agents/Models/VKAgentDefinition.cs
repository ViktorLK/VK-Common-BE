using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Declarative specification of an agent (instructions, model profile key, allowed tools,
/// execution guardrails, and chat parameter defaults) used to instantiate a runnable <see cref="IVKAgent"/>.
/// </summary>
public sealed record VKAgentDefinition
{
    /// <summary>
    /// Gets the unique name of the agent.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets a concise description of the agent's purpose and capabilities.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the base system instructions for the agent.
    /// </summary>
    public string Instructions { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional keyed model/provider profile name resolved at execution time.
    /// </summary>
    public string? ModelProfile { get; init; }

    /// <summary>
    /// Gets the optional allow-list of atomic tool names permitted for this agent.
    /// </summary>
    public IReadOnlyList<string>? AllowedToolNames { get; init; }

    /// <summary>
    /// Gets the explicit atomic tools attached to this agent definition.
    /// </summary>
    public IReadOnlyList<IVKAtomicTool>? Tools { get; init; }

    /// <summary>
    /// Gets the per-agent execution guardrail boundaries.
    /// </summary>
    public VKAgentGuardrailOptions? Guardrails { get; init; }

    /// <summary>
    /// Gets optional default chat sampling and model overrides for this agent.
    /// </summary>
    public VKChatArgs? ChatDefaults { get; init; }

    /// <summary>
    /// Gets optional custom metadata associated with this agent definition.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }
}
