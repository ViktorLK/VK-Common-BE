using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.VectorStore.VecEngine.Internal;

/// <summary>
/// Feature registration for the VecEngine.
/// </summary>
internal sealed partial class VecEngineFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKVecEngineOptions options)
    {
        _ = services;
        _ = options;
    }

    static partial void ValidateFeatureCustom(VKVecEngineOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
