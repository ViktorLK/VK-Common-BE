using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Orchestration.Internal;

/// <summary>
/// Handles the registration of the Orchestration feature.
/// </summary>
internal static class OrchestrationFeatureRegistration
{
    public static IVKAICognitiveBuilder Register(IVKAICognitiveBuilder builder)
    {
        IServiceCollection services = builder.Services;

        if (services.IsVKBlockRegistered<OrchestrationFeature>())
        {
            return builder;
        }

        VKOrchestrationOptions options = services.AddVKBlockOptions<VKOrchestrationOptions>(builder.Configuration);

        services.AddVKBlockMarker<OrchestrationFeature>();

        if (!options.Enabled)
        {
            return builder;
        }

        services.TryAddSingleton<IVKIntentNexus, DefaultIntentOrchestrator>();
        services.TryAddScoped<IVKThoughtStream, DefaultThoughtStream>();

        return builder;
    }
}
