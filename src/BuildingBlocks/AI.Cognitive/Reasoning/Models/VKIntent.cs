namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents the parsed intent of a user input.
/// </summary>
public enum VKIntent
{
    /// <summary>
    /// Unknown or unclassified intent.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// General conversation or chat.
    /// </summary>
    Chat = 1,

    /// <summary>
    /// Roleplay or character-based interaction.
    /// </summary>
    Roleplay = 2,

    /// <summary>
    /// Consulting, advice, or knowledge retrieval.
    /// </summary>
    Consulting = 3,

    /// <summary>
    /// Task execution or management.
    /// </summary>
    Task = 4,

    /// <summary>
    /// System command or administrative action.
    /// </summary>
    System = 5
}
