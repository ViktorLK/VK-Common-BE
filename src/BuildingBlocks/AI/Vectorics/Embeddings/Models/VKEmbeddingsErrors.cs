using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Standard error constants for the Embeddings feature.
/// </summary>
public static class VKEmbeddingsErrors
{
    /// <summary>
    /// Error returned when the embedding generation fails.
    /// </summary>
    public static readonly VKError GenerationFailed = new("AI.Embeddings.GenerationFailed", "The embedding generation failed.");

    /// <summary>
    /// Error returned when the input is invalid.
    /// </summary>
    public static readonly VKError InvalidInput = new("AI.Embeddings.InvalidInput", "The embedding input is invalid.");

    /// <summary>
    /// Error returned when the embedding feature is disabled in configuration.
    /// </summary>
    public static readonly VKError FeatureDisabled = new("AI.Embeddings.FeatureDisabled", "The embedding feature is disabled.");

    public static readonly VKError EndpointRequired = new("AI.Embeddings.EndpointRequired", "The endpoint is required for the selected provider.");
    public static readonly VKError ApiKeyRequired = new("AI.Embeddings.ApiKeyRequired", "The API key is required for the selected provider.");
    public static readonly VKError InvalidResponse = new("AI.Embeddings.InvalidResponse", "The server returned an invalid or empty embeddings response.");

    public static VKError EngineError(string message) => new("AI.Embeddings.EngineError", message);
}
