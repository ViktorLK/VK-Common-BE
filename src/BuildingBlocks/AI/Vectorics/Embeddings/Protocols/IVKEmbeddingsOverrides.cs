namespace VK.Blocks.AI;

/// <summary>
/// Defines embedding parameters that can be overridden at the request level.
/// </summary>
public interface IVKEmbeddingsOverrides :
    IVKAIProviderOverrides,
    IVKAIResilienceOverrides,
    IVKAIQuotaOverrides
{
    /// <summary>
    /// Gets the dimensions of the embedding vectors.
    /// </summary>
    int? Dimensions { get; init; }

    /// <summary>
    /// Gets the batch size for embedding generation.
    /// </summary>
    int? BatchSize { get; init; }
}
