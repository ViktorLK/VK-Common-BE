namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines persona settings that can be overridden at the request level.
/// </summary>
public interface IVKPersonaOverrides
{
    /// <summary>
    /// Gets the default persona identifier.
    /// </summary>
    string? DefaultPersonaId { get; init; }

    /// <summary>
    /// Gets the reserved tokens for persona context.
    /// </summary>
    int? PersonaReservedTokens { get; init; }

    /// <summary>
    /// Gets a value indicating whether dynamic persona switching is allowed.
    /// </summary>
    bool? AllowDynamicPersonaSwitching { get; init; }
}
