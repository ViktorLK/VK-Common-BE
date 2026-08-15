using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Model Catalog feature marker and registration hub.
/// Automatically provisions and populates the core <see cref="IVKModelCatalog"/> and fallback <see cref="IVKModelCatalogStore"/>.
/// </summary>
[VKFeature(typeof(VKAIBlock), OptionsType = typeof(VKCatalogOptions))]
internal sealed partial class CatalogFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKCatalogOptions options)
    {
        var catalog = new DefaultModelCatalog();

        // Register any custom models configured via options
        if (options.CustomModels is { Count: > 0 })
        {
            foreach (var custom in options.CustomModels)
            {
                catalog.Register(custom);
            }
        }

        services.TryAddSingleton<IVKModelCatalog>(catalog);
        services.TryAddSingleton<IVKModelCatalogStore, InMemoryModelCatalogStore>();
    }

    // [SG Hook] Optional validation hook
    static partial void ValidateFeatureCustom(VKCatalogOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
