using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.VectorStore.VecEngine.Internal;

/// <summary>
/// Feature registration for the VecEngine.
/// </summary>
internal sealed partial class VecEngineFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKVecEngineOptions options)
    {
        _ = services;
        _ = options;
    }

    static partial void ValidateCustom(VKVecEngineOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
