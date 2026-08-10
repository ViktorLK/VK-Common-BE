using VK.Blocks.VectorStore.VecEngine.Internal;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Feature registration for the VecEngine.
/// </summary>
[VKFeature(typeof(VKVectorStoreBlock), OptionsType = typeof(VKVecEngineOptions))]
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
