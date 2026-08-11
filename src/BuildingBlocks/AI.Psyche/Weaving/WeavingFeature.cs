using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Weaving.Internal;
using VK.Blocks.Core;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Weaving feature marker and registration hub.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Feature marker and DI registration hub containing no business logic.")]
[VKFeature(typeof(VKAIPsycheBlock), OptionsType = typeof(VKWeavingOptions), ArgsGenerationMode = VKArgsGenerationMode.Implicit)]
internal sealed partial class WeavingFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKWeavingOptions options)
    {
        _ = options;

        // Extractors are now handled by their respective modules (Echo, Persona, Knowledge)

        // Register weaving pipeline tasks
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingPipelineTask, DefaultPromptFormatterTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingPipelineTask, DefaultPromptTruncateTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingPipelineTask, DefaultFragmentReplacementTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingPipelineTask, DefaultCoordinateResolveTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKWeavingPipelineTask, DefaultTapestryWeavingTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultWeavingStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKWeavingOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
