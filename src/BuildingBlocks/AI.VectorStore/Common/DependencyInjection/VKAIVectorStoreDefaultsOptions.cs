using VK.Blocks.Core;
using VK.Blocks.AI.VectorStore.VectorStore.Models;

namespace VK.Blocks.AI.VectorStore.Common.DependencyInjection;

/// <summary>
/// Default configuration settings for the AI Vector Store building block.
/// These values serve as fallbacks for all Vector Store features.
/// </summary>
[VKFeature(typeof(VKAIVectorStoreBlock), GenerateValidator = true, Namespace = "VK.Blocks.AI.VectorStore.Common.DependencyInjection")]
public sealed partial record VKAIVectorStoreDefaultsOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets the type of the vector store.
    /// </summary>
    public VKAIVectorStoreType Type { get; init; } = VKAIVectorStoreType.InMemory;

    /// <summary>
    /// Gets the default collection name.
    /// </summary>
    public string DefaultCollection { get; init; } = "default";

    /// <summary>
    /// Gets the default maximum number of results to return.
    /// </summary>
    public int DefaultLimit { get; init; } = 5;

    /// <summary>
    /// Gets the default minimum similarity score (0.0 to 1.0).
    /// </summary>
    public float DefaultMinScore { get; init; } = 0.7f;
}
