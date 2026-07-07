namespace VK.Blocks.AI.Engram;

/// <summary>
/// Time modes determining how time elapsed is measured for forgetting curve calculations.
/// </summary>
public enum VKDecayTimeMode
{
    /// <summary>
    /// Decay is strictly based on wall-clock time elapsed.
    /// </summary>
    WallClock,

    /// <summary>
    /// Decay is based on the count of active sessions/interactions.
    /// </summary>
    SessionCount,

    /// <summary>
    /// Hybrid: Wall-clock time during idle periods, session counts during active periods.
    /// </summary>
    Hybrid
}
