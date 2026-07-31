namespace VK.Blocks.AI.Engram;

/// <summary>
/// Source type of a memory revision signal.
/// </summary>
public enum VKRevisionSourceType
{
    /// <summary>
    /// Implicitly inferred by LLM during conversation analysis.
    /// </summary>
    LLMInferred = 0,

    /// <summary>
    /// Explicit override instructed directly by user or application logic.
    /// </summary>
    UserExplicitOverride = 1,

    /// <summary>
    /// System rule or administrative correction.
    /// </summary>
    SystemRule = 2
}
