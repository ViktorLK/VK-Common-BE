using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Options for Turn Orchestration slice.
/// Follows BB.05.
/// </summary>
public sealed partial record VKTurnOrchestrationOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether Turn Orchestration is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
