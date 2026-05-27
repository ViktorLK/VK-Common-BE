using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Cognitive.Persona.Internal;

/// <summary>
/// Persona feature marker and registration hub.
/// </summary>
internal sealed partial class PersonaFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKPersonaOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKPersonaStore, InMemoryPersonaStore>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKOrchestrationPipelineStage, DefaultPersonaPipelineStage>());

        // Register non-generic extractor, renderer and formatter
        services.TryAddSingleton<IVKPersonaRenderer, DefaultPersonaRenderer>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPromptExtractor, DefaultPersonaPromptExtractor>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKPromptFormatter, DefaultPersonaFormatter>());
    }

    // [SG Hook]
    static partial void ValidateCustom(VKPersonaOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
