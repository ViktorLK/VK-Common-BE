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
public sealed record VKAISKOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:{VKAISKBlock.BlockName}";


    /// <summary>
    /// Gets or sets the organization identifier for the AI service.
    /// </summary>
    public string? OrgId { get; init; }

    /// <summary>
    /// Gets or sets the deployment name for Azure OpenAI.
    /// </summary>
    public string? DeploymentName { get; init; }



    /// <summary>
    /// Gets or sets the prompt template format.
    /// Defaults to <see cref="AISKTemplateFormat.Default"/>.
    /// </summary>
    public AISKTemplateFormat TemplateFormat { get; init; } = AISKTemplateFormat.Default;

    /// <summary>
    /// Gets or sets a value indicating whether to enable native SK planners.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool EnableNativePlanners { get; init; }


    /// <summary>
    /// Gets or sets the plugin options.
    /// </summary>
    public AISKPluginOptions Plugins { get; init; } = new();
}
