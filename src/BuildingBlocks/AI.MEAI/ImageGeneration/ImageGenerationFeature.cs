#pragma warning disable MEAI001

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.AI.MEAI.ImageGeneration.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI;

/// <summary>
/// ImageGeneration feature marker and registration hub for VK.Blocks.AI.MEAI.
/// </summary>
// [SG Marker]
[VKFeature(typeof(VKAIMEAIBlock), OptionsType = typeof(VKAIMEAIImageGenerationOptions))]
internal sealed partial class ImageGenerationFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAIMEAIImageGenerationOptions options)
    {
        _ = options;
        VKAIMEAIBlock.TryAddDefaultAndKeyedClient<IImageGenerator>(
            services,
            static (providerOptions, _, sp) => AIMEAIClientFactory.CreateImageGenerator(providerOptions, sp));

        services.Replace(ServiceDescriptor.Scoped<IVKImageGenerationEngine, DefaultImageGenerationEngine>());
    }
}
