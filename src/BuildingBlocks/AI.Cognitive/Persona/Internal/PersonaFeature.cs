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
        services.TryAddScoped<IVKPersonaStore, InMemoryPersonaStore>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKPersonaOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
