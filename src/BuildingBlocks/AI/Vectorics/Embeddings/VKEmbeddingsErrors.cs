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
}
