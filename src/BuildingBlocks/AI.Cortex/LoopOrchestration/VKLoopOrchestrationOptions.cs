using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Options for the Agent Loop Orchestration slice.
/// Follows [BB.05] Options Architecture.
/// </summary>
public sealed partial record VKLoopOrchestrationOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether Loop Orchestration is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the default maximum number of iterations allowed per loop execution before forced termination.
    /// </summary>
    public int DefaultMaxIterations { get; init; } = 5;
}
