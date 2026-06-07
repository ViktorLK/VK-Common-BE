using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Specifies the prompt template format for Semantic Kernel.
/// </summary>
public enum AISKTemplateFormat
{
    /// <summary>
    /// The default Semantic Kernel template format.
    /// </summary>
    Default,

    /// <summary>
    /// Handlebars template format.
    /// </summary>
    Handlebars,

    /// <summary>
    /// Liquid template format.
    /// </summary>
    Liquid
}

/// <summary>
/// Options for the Semantic Kernel building block connectivity.
/// </summary>
[VKFeature(typeof(VKAISKBlock))]
public sealed partial record VKAISKOptions : IVKToggleableBlockOptions
{

    /// <summary>
    /// Gets or sets a value indicating whether the Semantic Kernel feature is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;
}
