using System.Collections.Generic;

// // [AP.03] Public contract in root namespace carrying VK prefix
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// The final, immutable output returned from the prompt weaving pipeline.
/// Follows AP.01 (Sealed Record).
/// </summary>
public sealed record VKPromptTapestry
{
    /// <summary>
    /// Gets the final list of woven chat messages to be sent to the AI model.
    /// </summary>
    public required IReadOnlyList<VKChatMessage> Messages { get; init; }

    /// <summary>
    /// Gets the compiled system instructions or metaprompt, if any.
    /// </summary>
    public string? SystemInstructions { get; init; }

    /// <summary>
    /// Gets the estimated total number of tokens consumed by this tapestry.
    /// </summary>
    public int TotalEstimatedTokens { get; init; }
}
