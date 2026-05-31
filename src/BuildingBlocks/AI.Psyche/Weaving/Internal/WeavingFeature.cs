using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Pipeline;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Psyche.Weaving.Internal;

internal sealed partial class WeavingFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKWeavingOptions options)
    {
        _ = options;

        // Extractors are now handled by their respective modules (Echo, Persona, Knowledge)

        // Register weaving pipeline tasks
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingTask, DefaultPromptFormatterTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingTask, DefaultPromptTruncateTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingTask, DefaultTapestryWeavingTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingStage, DefaultWeavingStage>());

        // Register orchestration engine
        services.TryAddScoped<IVKWeavingTaskEngine, DefaultPromptWeavingEngine>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKWeavingOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
