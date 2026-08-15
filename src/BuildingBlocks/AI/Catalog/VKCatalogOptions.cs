using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Configuration settings for the Model Catalog feature.
/// Non-toggleable core infrastructure options.
/// Allows custom registration of proprietary/local model capabilities via configuration.
/// </summary>
public sealed partial record VKCatalogOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets custom model metadata definitions registered at startup.
    /// </summary>
    public IReadOnlyList<VKModelMetadata> CustomModels { get; init; } = [];
}
