namespace VK.Blocks.Core;

/// <summary>
/// Execution phase within a composite pipeline (Unspecified vs Before Terminal/Middleware vs After Terminal/Middleware).
/// </summary>
public enum VKPipelinePhase
{
    /// <summary>
    /// Phase is unspecified or irrelevant (default for generic composite tasks/jobs).
    /// </summary>
    None = 0,

    /// <summary>
    /// Component executes before the terminal action and middleware onion chain.
    /// </summary>
    Before = 1,

    /// <summary>
    /// Component executes after the terminal action and middleware onion chain.
    /// </summary>
    After = 2
}
