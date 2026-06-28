using System;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Strategy for compressing AI engrams.
/// </summary>
public sealed record VKChatSession
{
    public required VKChatSessionId Id { get; init; }
    public required VKPersonaId PersonaId { get; init; }
    public required string Summary { get; init; }
    public string? NarrativeSummary { get; init; }
    public string? StructuredFacts { get; init; }
    public string? RelationGraph { get; init; }
    public string? Timeline { get; init; }
    public string? Contradictions { get; init; }
    public string? ActionItems { get; init; }
    public string? ConfidenceAnnotations { get; init; }
    public string? PredictiveCues { get; init; }
    public float? Valence { get; init; }
    public float? Arousal { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
