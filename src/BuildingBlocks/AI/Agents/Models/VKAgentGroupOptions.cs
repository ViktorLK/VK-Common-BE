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
}
