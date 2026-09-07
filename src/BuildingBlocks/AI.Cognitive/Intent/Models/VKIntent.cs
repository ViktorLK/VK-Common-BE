namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Primary intent classification enumeration.
/// Explicitly declared underlying type : byte per [AP.01] and VK1102.
/// </summary>
public enum VKIntent : byte
{
    /// <summary>
    /// General conversation, greetings, chit-chat, or small talk.
    /// </summary>
    Chat = 0,

    /// <summary>
    /// Character-based roleplay, creative writing, or scenario immersion.
    /// </summary>
    Roleplay = 1,

    /// <summary>
    /// Advice seeking, factual queries, technical/domain consulting, venting.
    /// </summary>
    Consulting = 2,

    /// <summary>
    /// Task execution, goal planning, scheduling, administrative actions.
    /// </summary>
    Task = 3,

    /// <summary>
    /// System commands, meta-inquiries about AI configuration, control flow.
    /// </summary>
    System = 4
}
