using System.Collections.Generic;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// The final, immutable output returned from the prompt weaving pipeline.
/// Follows AP.01 (Sealed Record).
/// </summary>
public sealed record VKPromptTapestry
{
    public required IReadOnlyList<VKChatMessage> Messages { get; init; }
    public string? SystemInstructions { get; init; }
    public int TotalEstimatedTokens { get; init; }
}
