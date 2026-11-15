using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Additional prompt instructions, contextual messages, or variables supplied dynamically by an
/// <see cref="IVKAgentContextProvider"/> before an agent invocation.
/// </summary>
public sealed record VKAgentAdditionalContext
{
    /// <summary>
    /// Gets optional text appended to the agent's effective system instructions for the current run.
    /// </summary>
    public string? SystemInstructionsAppendix { get; init; }

    /// <summary>
    /// Gets optional contextual messages prepended into the conversation input for the current run.
    /// </summary>
    public IReadOnlyList<VKChatMessage>? ContextMessages { get; init; }

    /// <summary>
    /// Gets optional variables merged into the agent execution context.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Variables { get; init; }
}
