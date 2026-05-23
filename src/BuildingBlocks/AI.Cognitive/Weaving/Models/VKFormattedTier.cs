// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents a text fragment formatted into a structured model-compatible layer.
/// Follows AP.01 (Sealed Record).
/// </summary>
public sealed record VKFormattedTier
{
    public required VKChatRole Role { get; init; }
    public required string Content { get; init; }
    public required VKKnowledgePositions Position { get; init; }
    public required VKPromptTierType TierType { get; init; }
    public int Depth { get; init; } = 0;
}
