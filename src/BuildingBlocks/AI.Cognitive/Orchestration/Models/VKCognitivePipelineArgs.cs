using System;
using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Arguments for the cognitive pipeline execution.
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
public sealed record VKCognitivePipelineArgs : IVKAIArgs, IVKArgs<VKCognitivePipelineArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKCognitivePipelineArgs Empty { get; } = new();

    /// <inheritdoc />
    public string? ModelName { get; init; }

    /// <inheritdoc />
    public IDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();

    /// <inheritdoc />
    public TimeSpan? Timeout { get; init; }

    /// <inheritdoc />
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the persona identifier to use for thinking.
    /// </summary>
    public string? PersonaId { get; init; }

    /// <summary>
    /// Gets the preset identifier to use for task-specific static bottom notes.
    /// </summary>
    public string? PresetId { get; init; }

    /// <summary>
    /// Gets the retrieval arguments (Recall stage).
    /// </summary>
    public VKRetrievalArgs? RecallArgs { get; init; }

    /// <summary>
    /// Gets the agent execution arguments (Act stage).
    /// </summary>
    public VKAgentsArgs? ActionArgs { get; init; }

    /// <summary>
    /// Gets a value indicating whether to skip the Recall stage.
    /// </summary>
    public bool SkipRecall { get; init; } = false;

    /// <summary>
    /// Gets the maximum allowed reasoning tokens per streaming request before dynamic truncation occurs.
    /// </summary>
    public int? MaxReasoningTokens { get; init; }

    /// <summary>
    /// Gets the system instructions (persona core) for structured chat.
    /// </summary>
    public string? SystemInstructions { get; init; }

    /// <summary>
    /// Gets the structured chat history for role-aware execution.
    /// </summary>
    public IEnumerable<VKChatMessage>? ChatHistory { get; init; }

    /// <summary>
    /// Gets the physical and environmental world state coordinates for presence tracking.
    /// </summary>
    public VKWorldState? WorldState { get; init; }
}
