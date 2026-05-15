using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Persona.Internal;

/// <summary>
/// Handles the registration of the Persona feature.
/// </summary>
internal static class PersonaFeatureRegistration
{
    public static IVKAICognitiveBuilder Register(IVKAICognitiveBuilder builder)
    {
        IServiceCollection services = builder.Services;

        // 1. Idempotency Check
        if (services.IsVKBlockRegistered<PersonaFeature>())
        {
            return builder;
        }

        // 2. Options Registration
        VKPersonaOptions options = services.AddVKBlockOptions<VKPersonaOptions>(builder.Configuration);

        // 3. Mark-Self
        services.AddVKBlockMarker<PersonaFeature>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKPersonaOptions>, PersonaOptionsValidator>();

        // 5. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 6. Core Services
        // Implementations would go here.

        return builder;
    }
}
