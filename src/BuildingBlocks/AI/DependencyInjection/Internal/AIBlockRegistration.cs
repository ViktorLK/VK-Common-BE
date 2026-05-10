using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.DependencyInjection.Internal;

/// <summary>
/// Internal registration logic for the AI building block.
/// </summary>
internal static class AIBlockRegistration
{
    /// <summary>
    /// Registers the AI building block.
    /// </summary>
    internal static IVKAIBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAIOptions, VKAIOptions>? transform = null)
    {
        var builder = new AIBlockBuilder(services, configuration);

        // 1. Check-Self & Prerequisite
        // IsVKBlockRegistered handles dependency checking (VKCoreBlock)
        if (services.IsVKBlockRegistered<VKAIBlock>())
        {
            return builder;
        }

        // 2. Options Registration
        VKAIOptions options = services.AddVKBlockOptions(configuration, transform);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKAIBlock>();

        // 4. Options Validation
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<VKAIOptions>, AIOptionsValidator>());

        // 5. Diagnostics
        // (Automatically handled by [VKBlockDiagnostics] Source Generator)

        // 6. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 7. Core Services
        // Feature services are registered via builder extension methods (e.g., .AddVKChat())

        return builder;
    }
}
