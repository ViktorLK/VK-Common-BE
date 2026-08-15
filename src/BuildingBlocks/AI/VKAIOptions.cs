using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Root infrastructure configuration for the AI building block.
/// Governs transport channel and sensitive logging compliance.
/// Feature-specific settings (Chat, Embeddings) belong to their respective feature options.
/// </summary>
public sealed partial record VKAIOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets the custom named HttpClient used for AI transport pipelines.
    /// If null or empty, defaults to "VK.AI" or the anonymous HttpClient.
    /// </summary>
    public string? HttpClientName { get; init; }

    /// <summary>
    /// Gets or sets whether sensitive user prompts and completions can be emitted to telemetry/logs.
    /// Defaults to false for privacy & GDPR compliance.
    /// </summary>
    public bool EnableSensitiveDataLogging { get; init; } = false;
}
