namespace VK.Blocks.AI;

/// <summary>
/// Defines the role of a message in a chat conversation.
/// </summary>
public enum VKChatRole
{
    /// <summary>
    /// System role for instructions.
    /// </summary>
    System,

    /// <summary>
    /// User role for user input.
    /// </summary>
    User,

    /// <summary>
    /// Assistant role for AI responses.
    /// </summary>
    Assistant,

    /// <summary>
    /// Tool role for tool/function output.
    /// </summary>
    Tool
}
