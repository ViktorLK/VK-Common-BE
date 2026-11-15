using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Options for configuring a multi-agent group execution.
/// </summary>
[VKOptions]
public sealed record VKAgentGroupOptions
{
    /// <summary>
    /// Gets the maximum number of rounds before automatic termination.
    /// </summary>
    public int MaxRounds { get; init; } = 10;

    /// <summary>
    /// Gets the selection strategy for determining the next agent.
    /// </summary>
    public VKAgentSelectionMode SelectionMode { get; init; } = VKAgentSelectionMode.RoundRobin;

    /// <summary>
    /// Gets the termination keywords that signal completion.
    /// </summary>
    public IReadOnlyList<string> TerminationKeywords { get; init; } = ["APPROVE", "DONE", "TERMINATE"];

    /// <summary>
    /// Gets a value indicating whether <see cref="VKAgentSelectionMode.LLMBased"/> may fall back
    /// to <see cref="VKAgentSelectionMode.RoundRobin"/> when selection fails or no selector client is available.
    /// Defaults to false (AP.06: fail explicitly unless fallback is explicitly opted in).
    /// </summary>
    public bool AllowFallbackToRoundRobin { get; init; }

    /// <summary>
    /// Gets optional directional handoff routes keyed by source agent name mapping to allowed target agent names
    /// when executing in <see cref="VKAgentSelectionMode.Handoff"/> mode.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? HandoffRoutes { get; init; }
}
