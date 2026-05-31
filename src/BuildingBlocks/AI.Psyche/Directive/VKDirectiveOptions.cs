using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Options for the Tenant Directive feature.
/// Follows BB.05 (Options pattern with sealed record).
/// </summary>
[VKFeature(typeof(VKAIPsycheBlock))]
public sealed partial record VKDirectiveOptions : IVKDirectiveOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the Tenant Directive feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
