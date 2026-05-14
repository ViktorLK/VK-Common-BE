using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Memory.Internal;

/// <summary>
/// Handles the registration of the Memory feature.
/// </summary>
internal static class MemoryFeatureRegistration
{
    public static IVKAICognitiveBuilder Register(IVKAICognitiveBuilder builder)
    {
        IServiceCollection services = builder.Services;

        // 1. Idempotency Check
        if (services.IsVKBlockRegistered<MemoryFeature>())
        {
            return builder;
        }

        // 2. Options Registration
        VKMemoryOptions options = services.AddVKBlockOptions<VKMemoryOptions>(builder.Configuration);

        // 3. Mark-Self
        services.AddVKBlockMarker<MemoryFeature>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKMemoryOptions>, MemoryOptionsValidator>();

        // 5. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 6. Core Services
        services.TryAddScoped<IVKRealityLedger, VKAICognitiveRealityLedger>();
        services.TryAddScoped<IVKGraphMemory, NullGraphMemory>();
        services.TryAddScoped<IVKStructuredMemory, NullStructuredMemory>();

        return builder;
    }
}
