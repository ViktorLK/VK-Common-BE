namespace VK.Blocks.AI;

/// <summary>
/// Registry protocol for resolving physical AI model capabilities dynamically.
/// </summary>
public interface IVKModelCatalog
{
    /// <summary>
    /// Gets physical metadata and context window limits for a given model ID.
    /// </summary>
    VKModelMetadata GetModelMetadata(string modelId);

    /// <summary>
    /// Registers or overrides physical metadata for a model in the catalog.
    /// </summary>
    void Register(VKModelMetadata metadata);
}
