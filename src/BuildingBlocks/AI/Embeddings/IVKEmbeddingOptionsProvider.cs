namespace VK.Blocks.AI;

/// <summary>
/// Defines a provider for resolving <see cref="VKEmbeddingOptions"/> dynamically.
/// </summary>
public interface IVKEmbeddingOptionsProvider
{
    /// <summary>
    /// Gets the current embedding options, potentially resolved from a dynamic context.
    /// </summary>
    VKEmbeddingOptions GetOptions();
}
