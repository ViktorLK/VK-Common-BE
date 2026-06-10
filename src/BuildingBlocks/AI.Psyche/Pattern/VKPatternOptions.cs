using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Configuration settings for the Pattern feature.
/// </summary>
[VKFeature(typeof(VKAIPsycheBlock))]
public sealed partial record VKPatternOptions : IVKPatternOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Pattern feature is enabled.
    /// Defaults to true.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
