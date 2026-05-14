using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Configuration settings for the Persona feature.
/// </summary>
public sealed record VKPersonaOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name for Persona options.
    /// </summary>
    public static string SectionName => VKAICognitiveOptions.SectionName + ":" + VKAICognitiveOptions.PersonaSection;

    /// <summary>
    /// Gets or sets a value indicating whether Persona feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the default persona identifier.
    /// </summary>
    public string? DefaultPersonaId { get; init; }

    public int PersonaReservedTokens { get; init; } = 512;
    public bool AllowDynamicPersonaSwitching { get; init; } = true;

}
