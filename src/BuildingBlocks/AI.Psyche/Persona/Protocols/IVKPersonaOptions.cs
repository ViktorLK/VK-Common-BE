using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Aggregates all static Persona configuration options.
/// </summary>
public interface IVKPersonaOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets the reserved tokens for persona context.
    /// </summary>
    int PersonaReservedTokens { get; }

    /// <summary>
    /// Gets a value indicating whether dynamic persona switching is allowed.
    /// </summary>
    bool AllowDynamicPersonaSwitching { get; }
}
