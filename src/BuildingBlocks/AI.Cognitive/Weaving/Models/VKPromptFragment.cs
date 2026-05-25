// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents a raw, atomic text fragment extracted from a source.
/// Follows AP.01 (Sealed Record).
/// </summary>
public sealed record VKPromptFragment
{
    public required string Id { get; init; }
    public required string Content { get; init; }
    public required VKKnowledgePositions Position { get; init; }
    public required VKPromptTierType TierType { get; init; }
    public int Priority { get; init; } = 0;
    public int Depth { get; init; } = 0;
    public string? InclusionGroup { get; init; }
    public int GroupWeight { get; init; } = 100;
    public object? Metadata { get; init; }
}
