using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// Handles the registration of the Agents feature.
/// </summary>
internal static class AgentsFeatureRegistration
{
    public static IVKAIBuilder Register(IVKAIBuilder builder)
    {
        IServiceCollection services = builder.Services;

        // 1. Idempotency Check
        if (services.IsVKBlockRegistered<AgentsFeature>())
        {
            return builder;
        }

        // 2. Options Registration
        VKAgentOptions options = services.AddVKBlockOptions<VKAgentOptions>(builder.Configuration);

        // 3. Mark-Self
        services.AddVKBlockMarker<AgentsFeature>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKAgentOptions>, AgentsOptionsValidator>();

        // 5. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 6. Core Services
        services.AddScoped<IVKAgentOptionsProvider, VKAgentDefaultOptionsProvider>();

        return builder;
    }
}
