using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.ImageGeneration.Internal;

/// <summary>
/// Image Generation feature marker and registration hub.
/// </summary>
internal sealed partial class ImageGenerationFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKImageGenerationOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKImageGenerationEngine, NoOpVKImageGenerationEngine>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKImageGenerationOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
