using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Cognitive.Memory.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Common.DependencyInjection.Internal;

internal static class VKAICognitiveBlockRegistration
{
    internal static IVKAICognitiveBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAICognitiveOptions, VKAICognitiveOptions>? transform = null)
    {
        var builder = new VKAICognitiveBlockBuilder(services, configuration.GetSection(VKBlocksConstants.VKBlocksConfigPrefix));

        if (services.IsVKBlockRegistered<VKAICognitiveBlock>())
        {
            return builder;
        }

        VKAICognitiveOptions options = services.AddVKBlockOptions(configuration, transform);

        services.AddVKBlockMarker<VKAICognitiveBlock>();

        // 4. Options Validation
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<VKAICognitiveOptions>, VKAICognitiveOptionsValidator>());

        // 5. Early Return Check
        if (!options.Enabled)
        {
            return builder;
        }

        // 6. Core Services
        services.TryAddScoped<IVKMemoryLedger, BasicMemoryLedger>();

        return builder;
    }
}
