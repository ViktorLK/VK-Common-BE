using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Feature marker and registration hook for the Presence feature.
/// Following BB.02 and BB.06.
/// </summary>
internal sealed partial class PresenceFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKPresenceOptions options) // [SG Hook]
    {
        // 1. Ensure TimeProvider is registered as fallback
        services.TryAddSingleton(TimeProvider.System);

        // 2. Register core Presence services
        services.TryAddScoped<IVKPresenceStressMonitor, PresenceStressMonitor>();
        services.TryAddScoped<IVKPresenceSelfMonitor, PresenceSelfMonitor>();

        // 3. Register proactive background hosted service and public callback interface
        services.TryAddSingleton<PresenceProactiveEngine>();
        services.TryAddSingleton<IVKPresenceProactiveEngine>(sp => sp.GetRequiredService<PresenceProactiveEngine>());
        services.AddHostedService(sp => sp.GetRequiredService<PresenceProactiveEngine>());
    }

    static partial void ValidateCustom(VKPresenceOptions options, List<string> failures) // [SG Hook]
    {
        if (options.Enabled && (options.SentimentThreshold < 0f || options.SentimentThreshold > 1f))
        {
            failures.Add("Presence.SentimentThreshold must be between 0.0 and 1.0.");
        }
    }
}
