using VK.Blocks.AI;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Execution parameters for AI Synapse route dispatching.
/// Follows AP.05 Args pattern with standard strongly-typed provider and model options.
/// </summary>
public sealed record VKAIRouteArgs
{
    /// <summary>
    /// Gets the operation name for logging and OpenTelemetry tracing (e.g. "Chat", "Embeddings").
    /// </summary>
    public string OperationKey { get; init; } = "AI.Execute";

    /// <summary>
    /// Gets the preferred provider type (e.g. OpenAI, Azure, Anthropic).
    /// </summary>
    public VKAIProviderType? PreferredProvider { get; init; }

    /// <summary>
    /// Gets the preferred model ID (e.g. VKAIModelIds.OpenAI.Gpt4OMini).
    /// </summary>
    public string? PreferredModelId { get; init; }
}
