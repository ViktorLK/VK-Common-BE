using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.Pipeline;

/// <summary>
/// Options for the AI Psyche Pipeline.
/// </summary>
[VKFeature(typeof(VKAIPsycheBlock), GenerateArgs = true, GenerateValidator = true)]
public sealed partial record VKPipelineOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Pipeline feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether strict ordering should be enforced.
    /// </summary>
    public bool EnforceStrictOrdering { get; init; } = true;
}
