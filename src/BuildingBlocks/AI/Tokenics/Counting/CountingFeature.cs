using VK.Blocks.AI.Tokenics.Counting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Token Counting feature marker and registration hub.
/// </summary>
[VKFeature(typeof(global::VK.Blocks.AI.TokenicsFeature), OptionsType = typeof(VKCountingOptions))]
internal sealed partial class CountingFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKCountingOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKTokenCounter, DefaultTokenCounter>();
    }

    /// <summary>Add counting-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKCountingOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
