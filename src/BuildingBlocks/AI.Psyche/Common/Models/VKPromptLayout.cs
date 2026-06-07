using System.Collections.Generic;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Dictates the immutable physical layout coordinates for weaving assembly.
/// Keeps options classes clean and single-purpose.
/// </summary>
internal static class PromptLayout
{
    /// <summary>
    /// Gets the default render order boundaries for each prompt tier type.
    /// Higher values indicate that the tier will be rendered later in the sequence.
    /// </summary>
    internal static IReadOnlyDictionary<VKPromptTierType, int> DefaultRenderOrders { get; } =
        new Dictionary<VKPromptTierType, int>
        {
            [VKPromptTierType.Directive] = 0,
            [VKPromptTierType.Persona] = 1000,
            [VKPromptTierType.Knowledge] = 2000,
            [VKPromptTierType.Echo] = 3000,
        };
}
