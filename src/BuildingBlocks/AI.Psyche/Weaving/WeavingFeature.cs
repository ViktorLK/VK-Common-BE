using VK.Blocks.AI.Psyche.Weaving.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Psyche;

[VKFeature(typeof(VKAIPsycheBlock), OptionsType = typeof(VKWeavingOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class WeavingFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKWeavingOptions options)
    {
        _ = options;

        // Extractors are now handled by their respective modules (Echo, Persona, Knowledge)

        // Register weaving pipeline tasks
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingTask, DefaultPromptFormatterTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingTask, DefaultPromptTruncateTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingTask, DefaultFragmentReplacementTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingTask, DefaultCoordinateResolveTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingTask, DefaultTapestryWeavingTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheBeforePipelineStage, DefaultWeavingStage>());

        // Register orchestration engine
        services.TryAddScoped<IVKWeavingTaskEngine, DefaultPromptWeavingEngine>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKWeavingOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
