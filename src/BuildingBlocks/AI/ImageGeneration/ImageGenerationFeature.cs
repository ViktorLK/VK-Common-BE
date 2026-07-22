using VK.Blocks.AI.ImageGeneration.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Image Generation feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIBlock), OptionsType = typeof(VKImageGenerationOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit, ArgsBaseType = typeof(IVKAIArgs))]
internal sealed partial class ImageGenerationFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKImageGenerationOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKImageGenerationEngine, NoOpVKImageGenerationEngine>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKImageGenerationOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
