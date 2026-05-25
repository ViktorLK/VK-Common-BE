// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Combines an extracted VKPromptFragment with its computed weights and stickiness.
/// Follows AP.01 (Sealed Record).
/// </summary>
public sealed record VKScoredFragment
{
    public required VKPromptFragment Fragment { get; init; }
    public required double Score { get; init; }
    public bool IsSticky { get; init; } = false;
}
