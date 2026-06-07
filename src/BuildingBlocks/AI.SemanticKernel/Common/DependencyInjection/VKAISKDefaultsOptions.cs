using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;

/// <summary>
/// Default options for Semantic Kernel implementations.
/// </summary>
[VKFeature(typeof(VKAISKBlock), GenerateValidator = true, Namespace = "VK.Blocks.AI.SemanticKernel.Common.DependencyInjection")]
public sealed partial record VKAISKDefaultsOptions : IVKBlockOptions
{

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
    public VKAISKPluginOptions Plugins { get; init; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to enable native kernel caching.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool EnableKernelCaching { get; init; } = true;
}
