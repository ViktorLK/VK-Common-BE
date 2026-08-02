namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Specifies the session operational mode for dialogue boundary handling and memory inheritance.
/// Located in VK.Blocks.AI.Psyche as Psyche is the single source of truth for session and dialogue context management.
/// </summary>
public enum VKSessionMode
{
    /// <summary>
    /// Isolated session (default): L1 echo history and L2 summary memories are strictly isolated within this SessionId.
    /// </summary>
    Isolated = 0,

    /// <summary>
    /// Continuous session: Automatically traces single-level ParentSessionId to inherit parent Echo history and L2 summary.
    /// </summary>
    Continuous = 1,

    /// <summary>
    /// Sandbox / Trial session: Runs normally in execution context, but strictly bypasses L2 summary distillation and L3 fact consolidation.
    /// Does not pollute long-term memory store.
    /// </summary>
    Sandbox = 2
}
