using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.VectorSearch.Fusion.Internal;

namespace VK.Blocks.VectorSearch.Fusion.Internal;

/// <summary>
/// Feature registration for the Fusion capability.
/// </summary>
internal sealed partial class FusionFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKFusionOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKScoreFusion, ReciprocalRankFusion>();
    }

    static partial void ValidateCustom(VKFusionOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
