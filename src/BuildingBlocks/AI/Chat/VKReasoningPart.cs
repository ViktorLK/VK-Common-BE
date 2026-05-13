namespace VK.Blocks.AI;

/// <summary>
/// Represents the reasoning or thinking process of an AI model (e.g., Chain of Thought).
/// </summary>
public sealed record VKReasoningPart : IVKChatMessagePart
{
    /// <inheritdoc />
    public string PartType => "reasoning";

    /// <summary>
    /// Gets the reasoning text content.
    /// </summary>
    public required string Reasoning { get; init; }
}
