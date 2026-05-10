using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel;

/// <summary>
/// Options for the Semantic Kernel building block connectivity.
/// </summary>
public sealed record VKAISKOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:AI:AISK";

    /// <summary>
    /// Gets or sets a value indicating whether the Semantic Kernel block is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets or sets the API endpoint for the AI service.
    /// </summary>
    public string? Endpoint { get; init; }

    /// <summary>
    /// Gets or sets the API key for the AI service.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// Gets or sets the organization identifier for the AI service.
    /// </summary>
    public string? OrgId { get; init; }

    /// <summary>
    /// Gets or sets the deployment name for Azure OpenAI.
    /// </summary>
    public string? DeploymentName { get; init; }

    /// <summary>
    /// Gets or sets the service type (e.g., "OpenAI", "AzureOpenAI").
    /// Defaults to "OpenAI".
    /// </summary>
    public string ServiceType { get; init; } = "OpenAI";
}
