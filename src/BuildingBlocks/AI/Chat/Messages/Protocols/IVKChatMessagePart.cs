namespace VK.Blocks.AI;

/// <summary>
/// Defines a part of a multi-modal chat message.
/// </summary>
public interface IVKChatMessagePart
{
    /// <summary>
    /// Gets the type of the part.
    /// </summary>
    string PartType { get; }
}
