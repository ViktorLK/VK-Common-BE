using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

/// <summary>
/// Root options for VK.Blocks.AI.Eidos module.
/// Follows [BB.05] Options Architecture (sealed partial record).
/// </summary>
public sealed partial record VKAIEidosOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether the AI.Eidos block is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
