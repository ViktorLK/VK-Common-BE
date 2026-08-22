using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.Validation.Pipeline.Internal;
using VK.Blocks.Validation.Validators.Internal;

namespace VK.Blocks.Validation;

/// <summary>
/// A marker type for the VK.Blocks.Validation building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKValidationBlock : IVKBlockMarker
{
    static partial void RegisterBlockCustom(IVKValidationBuilder builder)
    {
        var services = builder.Services;
        var options = services.GetVKServiceInstance<VKValidationOptions>()!;

        if (options.EnableDataAnnotations)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKValidator, DataAnnotationsValidator>());
        }

        services.TryAddScoped<IVKValidationPipeline, ValidationPipeline>();
    }
}
