using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Common.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the AI building block.
/// Following BB.03.2 execution sequence and industrial patterns.
/// </summary>
internal static class AIBlockRegistration
{
    internal static IVKAIBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAIOptions, VKAIOptions>? transform = null)
    {
        VKGuard.NotNull(services);

        // 1. Check-Self (Idempotency) [AP.02]
        if (services.IsVKBlockRegistered<VKAIBlock>())
        {
            return new AIBlockBuilder(services, configuration);
        }

        // 2. Options Registration [BB.06]
        _ = services.AddVKBlockOptions<VKAIOptions>(configuration!, transform);

        // 3. Mark-Self (Metadata) [AP.02]
        services.AddVKBlockMarker<VKAIBlock>();

        // 4. Initialize Builder
        var builder = new AIBlockBuilder(services, configuration);

        // 5. Default Feature Opt-in
        // Automatically enable core defaults (Provider, Retry, etc.)
        return builder.AddVKDefaults();
    }
}
