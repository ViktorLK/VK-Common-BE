using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Text.Internal;

/// <summary>
/// Handles the registration of the Text feature.
/// </summary>
internal static class TextFeatureRegistration
{
    public static IVKAIBuilder Register(
        IVKAIBuilder builder,
        Func<VKTextOptions, VKTextOptions>? transform = null)
    {
        IServiceCollection services = builder.Services;

        // 1. Idempotency Check
        if (services.IsVKBlockRegistered<TextFeature>())
        {
            return builder;
        }

        // 2. Options Registration
        VKTextOptions options = services.AddVKBlockOptions<VKTextOptions>(builder.Configuration!, transform);

        // 3. Mark-Self
        services.AddVKBlockMarker<TextFeature>();

        // 4. Options Validation
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<VKTextOptions>, TextOptionsValidator>());

        // 5. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 6. Core Services
        services.TryAddSingleton<IVKTextEngine, NoOpVKTextEngine>();

        return builder;
    }
}
