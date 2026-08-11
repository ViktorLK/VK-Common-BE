using System.Collections.Generic;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Session-level knowledge execution state tracking last evaluated turns and trigger history.
/// Follows AP.01 (sealed record default).
/// </summary>
public sealed record VKSessionKnowledgeState
{
    /// <summary>
    /// Gets the last evaluated dialogue turn index.
    /// </summary>
    public int LastEvaluatedTurn { get; init; }

    /// <summary>
    /// Gets the map of strongly-typed VKKnowledgeId to their last triggered turn index.
    /// </summary>
    public IReadOnlyDictionary<VKKnowledgeId, int> LastTriggeredTurns { get; init; } = new Dictionary<VKKnowledgeId, int>();
}
