using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Assessment result produced by cognitive reflection.
/// Shared DTO definition located in VK.Blocks.AI contract library for zero-coupling cross-block access.
/// </summary>
public sealed record VKReflectionAssessment
{
    /// <summary>
    /// Evaluated importance score of the session or turn (0.0 to 1.0).
    /// Used by Engram to select compression strategy.
    /// </summary>
    public double ImportanceScore { get; init; } = 0.5;

    /// <summary>
    /// Extracted key facts from the interaction.
    /// </summary>
    public IReadOnlyList<string> FactExtractions { get; init; } = [];

    /// <summary>
    /// Evaluated knowledge proposals with double-axis lifecycle controls and confidence scores for corpus ingestion.
    /// </summary>
    public IReadOnlyList<VKKnowledgeProposal> KnowledgeProposals { get; init; } = [];

    /// <summary>
    /// Evaluated trait or state deltas to be applied to Somatic state.
    /// </summary>
    public IReadOnlyDictionary<string, double> TraitDeltas { get; init; } = new Dictionary<string, double>();

    /// <summary>
    /// Notable moment summary if this session contained a significant milestone.
    /// </summary>
    public string? NotableMoment { get; init; }
}
