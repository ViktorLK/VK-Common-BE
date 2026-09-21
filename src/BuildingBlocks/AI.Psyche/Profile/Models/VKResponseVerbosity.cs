namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Defines the level of explanation detail and output verbosity.
/// Follows AP.01 and AP.03.
/// </summary>
public enum VKResponseVerbosity : byte
{
    /// <summary>
    /// Standard balanced response length and detail.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Highly concise, key-points and code-first, omitting pleasantries and boilerplate.
    /// </summary>
    Concise = 1,

    /// <summary>
    /// Detailed and comprehensive explanations, providing full context and background.
    /// </summary>
    Detailed = 2,

    /// <summary>
    /// Structured step-by-step procedural tutorial format.
    /// </summary>
    StepByStep = 3
}
