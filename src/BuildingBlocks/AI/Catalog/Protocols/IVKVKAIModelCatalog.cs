namespace VK.Blocks.AI;

/// <summary>
/// Registry protocol for resolving physical AI model capabilities dynamically.
/// </summary>
public interface IVKVKAIModelCatalog
{
    /// <summary>
    /// Gets physical metadata and context window limits for a given provider and model ID.
    /// </summary>
    VKAIModelMetadata GetAIModelMetadata(VKAIProviderType provider, string modelId);

    /// <summary>
    /// Registers or overrides physical metadata for a model in the catalog.
    /// </summary>
    void Register(VKAIModelMetadata metadata);
}
