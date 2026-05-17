using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Root configuration settings for all AI Guardrails (Content, Privacy, Injections).
/// Following AP.05: Hierarchical configuration pattern.
/// </summary>
[VKFeature(typeof(VKAIBlock))]
public sealed partial record VKGuardrailsOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether all Guardrails features are enabled.
    /// Defaults to false.
    /// </summary>
    public bool Enabled { get; init; } = false;
}
