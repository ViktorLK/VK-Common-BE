using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Configuration settings for the Persona feature.
/// </summary>
[VKFeature(typeof(VKAIPsycheBlock), GenerateArgs = true)]
public sealed partial record VKPersonaOptions : IVKPersonaOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Persona feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the reserved tokens for persona context.
    /// </summary>
    public int PersonaReservedTokens { get; init; } = 512;

    /// <summary>
    /// Gets a value indicating whether dynamic persona switching is allowed.
    /// </summary>
    public bool AllowDynamicPersonaSwitching { get; init; } = true;
}
