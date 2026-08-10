using VK.Blocks.VectorSearch.Fusion.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Feature registration for the Fusion capability.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock), OptionsType = typeof(VKFusionOptions))]
internal sealed partial class FusionFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKFusionOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKScoreFusion, ReciprocalRankFusion>();
    }

    static partial void ValidateFeatureCustom(VKFusionOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
