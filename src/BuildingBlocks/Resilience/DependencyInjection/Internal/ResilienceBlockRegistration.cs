using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Resilience.DependencyInjection.Internal;

internal static class ResilienceBlockRegistration
{
    public static IVKResilienceBuilder Register(IServiceCollection services, IConfiguration configuration)
    {
        var builder = new VKResilienceBuilder(services);

        // 1. Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKResilienceBlock>())
        {
            return builder;
        }

        // 2. Options Registration
        var options = services.AddVKBlockOptions<VKResilienceOptions>(configuration);

        // 3. Mark-Self
        services.AddVKBlockMarker<VKResilienceBlock>();

        // 4. Options Validation
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<VKResilienceOptions>, ResilienceOptionsValidator>());

        // 5. Diagnostics

        // 6. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 7. Core Services
        return builder;
    }
}
