namespace VK.Blocks.AI;

/// <summary>
/// Represents a text part of a message.
/// </summary>
public sealed record VKTextPart : IVKChatMessagePart
{
    /// <inheritdoc />
    public string PartType => "text";

    /// <summary>
    /// Gets the text content.
    /// </summary>
    public required string Text { get; init; }
}
