using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.VectorStore.Common.DependencyInjection.Internal;

/// <summary>
/// Hook implementation for VectorStore Defaults validation.
/// </summary>
internal sealed partial class VectorStoreDefaultsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKVectorStoreDefaultsOptions options)
    {
        _ = services;
        _ = options;
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKVectorStoreDefaultsOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.DefaultCollection))
        {
            failures.Add("DefaultCollection name cannot be empty.");
        }

        if (options.DefaultLimit <= 0)
        {
            failures.Add("DefaultLimit must be greater than zero.");
        }

        if (options.DefaultMinScore < 0.0f || options.DefaultMinScore > 1.0f)
        {
            failures.Add("DefaultMinScore must be between 0.0 and 1.0.");
        }
    }
}
