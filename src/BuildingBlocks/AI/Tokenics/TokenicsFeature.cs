using VK.Blocks.AI.Tokenics.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Tokenics.Counting.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Tokenics feature marker and registration hub.
/// </summary>
[VKFeature(typeof(global::VK.Blocks.AI.VKAIBlock), OptionsType = typeof(VKTokenicsOptions))]
internal sealed partial class TokenicsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKTokenicsOptions options)
    {
        services.TryAddSingleton<IVKTokenCounter, DefaultTokenCounter>();
        _ = options;
    }

    /// <summary>Add tokenics-level validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKTokenicsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
