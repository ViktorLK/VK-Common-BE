using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI;

/// <summary>
/// AtomicTools feature marker and registration hub for VK.Blocks.AI.MEAI.
/// </summary>
// [SG Marker]
[VKFeature(typeof(VKAIMEAIBlock), OptionsType = typeof(VKAIMEAIAtomicToolsOptions))]
internal sealed partial class AtomicToolsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAIMEAIAtomicToolsOptions options)
    {
        _ = services;
        _ = options;
    }
}

