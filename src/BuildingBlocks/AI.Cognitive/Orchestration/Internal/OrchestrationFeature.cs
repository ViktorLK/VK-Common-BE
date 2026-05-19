using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Cognitive.Orchestration.Internal;

/// <summary>
/// Orchestration feature marker and registration hub.
/// </summary>
internal sealed partial class OrchestrationFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKOrchestrationOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKIntentNexus, DefaultIntentOrchestrator>();
        services.TryAddScoped<IVKThoughtStream, DefaultThoughtStream>();
        services.TryAddScoped<IVKCognitivePipeline, DefaultCognitivePipeline>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKOrchestrationOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
