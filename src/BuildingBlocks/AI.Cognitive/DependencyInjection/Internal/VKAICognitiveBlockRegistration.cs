using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Cognitive.Memory.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.DependencyInjection.Internal;

internal static class VKAICognitiveBlockRegistration
{
    internal static IVKAICognitiveBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAICognitiveOptions, VKAICognitiveOptions>? transform = null)
    {
        var builder = new VKAICognitiveBlockBuilder(services, configuration);

        if (services.IsVKBlockRegistered<VKAICognitiveBlock>())
        {
            return builder;
        }

        VKAICognitiveOptions options = services.AddVKBlockOptions(configuration, transform);

        services.AddVKBlockMarker<VKAICognitiveBlock>();

        // 4. Options Validation
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<VKAICognitiveOptions>, VKAICognitiveOptionsValidator>());

        // 5. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 6. Core Services
        services.TryAddScoped<IVKRealityLedger, VKAICognitiveRealityLedger>();

        return builder;
    }
}
