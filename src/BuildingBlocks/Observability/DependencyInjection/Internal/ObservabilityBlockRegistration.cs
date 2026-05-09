using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Observability.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the Observability block.
/// Complies with BB.03.2.
/// </summary>
internal static class ObservabilityBlockRegistration
{
    internal static IServiceCollection Register(IServiceCollection services, IConfiguration configuration)
    {
        // 1. Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKObservabilityBlock>())
        {
            return services;
        }

        // 2. Options Registration
        var options = services.AddVKBlockOptions<VKObservabilityOptions>(configuration);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKObservabilityBlock>();

        // 4. Options Validation
        services.AddOptions<VKObservabilityOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // 5. Diagnostics
        // Source generated diagnostics handled by [VKBlockDiagnostics]

        // 6. Feature Toggle
        if (!options.Enabled)
        {
            return services;
        }

        // 7. Core Services
        services.TryAddTransient<IVKLogEnricher, VKApplicationEnricher>();
        services.TryAddTransient<IVKLogEnricher, VKUserContextEnricher>();
        services.TryAddTransient<IVKLogEnricher, VKTraceContextEnricher>();
        services.TryAddTransient<IVKLogContextEnricher, VKActivityLogContextEnricher>();

        return services;
    }
}

