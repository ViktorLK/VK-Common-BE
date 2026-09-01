using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Persona.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Persona feature marker and registration hub.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Feature marker and DI registration hub containing no business logic.")]
[VKFeature(typeof(VKAIPsycheBlock), OptionsType = typeof(VKPersonaOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class PersonaFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKPersonaOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddSingleton<InMemoryPersonaRepository>();
        services.TryAddSingleton<IVKPsychePersonaRepository>(sp => sp.GetRequiredService<InMemoryPersonaRepository>());
        services.TryAddSingleton<IVKReadRepository<VKPersonaAnchor, VKPersonaId>>(sp => sp.GetRequiredService<InMemoryPersonaRepository>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultPersonaStage>());

        // Register non-generic extractor, renderer and formatter
        services.TryAddSingleton<IVKPersonaRenderer, DefaultPersonaRenderer>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IVKPromptFormatter, DefaultPersonaFormatter>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKPersonaOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
