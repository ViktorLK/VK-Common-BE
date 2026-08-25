namespace VK.Blocks.AI;

/// <summary>
/// Defines the role of a message in a chat conversation.
/// </summary>
public enum VKChatRole : byte
{
    /// <summary>
    /// System role for instructions.
    /// </summary>
    System = 0,

    /// <summary>
    /// User role for user input.
    /// </summary>
    User = 1,

    /// <summary>
    /// Assistant role for AI responses.
    /// </summary>
    Assistant = 2,

    /// <summary>
    /// Tool role for tool/function output.
    /// </summary>
    Tool = 3
}
