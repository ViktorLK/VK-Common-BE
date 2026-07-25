using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Gateway.Internal;

namespace VK.Blocks.AI.Gateway;

/// <summary>
/// A marker type for the VK.Blocks.AI.Gateway building block.
/// </summary>
[ExcludeFromCodeCoverage]
[VKBlockMarker(Dependencies = [typeof(VKAIBlock)], Toggleable = false)]
public sealed partial class VKAIGatewayBlock
{

    static partial void RegisterBlockCustom(IVKAIGatewayBuilder builder)
    {
        var services = builder.Services;

        services.TryAddSingleton<IAICircuitBreaker, LocalAICircuitBreaker>();
        services.TryAddSingleton<IAIRateLimiter, LocalAIRateLimiter>();
        services.TryAddSingleton<IAIMetricsCollector, LocalAIMetricsCollector>();
        services.TryAddSingleton<IVKAIProviderTracker, AIProviderTracker>();
    }

}
