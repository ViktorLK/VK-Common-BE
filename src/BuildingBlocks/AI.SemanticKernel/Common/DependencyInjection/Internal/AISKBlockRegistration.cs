using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;

namespace VK.Blocks.AI.SemanticKernel.Common.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the Semantic Kernel building block.
/// Following BB.03.2 execution sequence and industrial patterns.
/// </summary>
internal static class AISKBlockRegistration
{
    internal static IVKAISKBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAISKOptions, VKAISKOptions>? transform = null)
    {
        VKGuard.NotNull(services);

        // 1. Check-Self (Idempotency) [AP.02]
        if (services.IsVKBlockRegistered<VKAISKBlock>())
        {
            return new AISKBlockBuilder(services, configuration.GetSection(VKBlocksConstants.VKBlocksConfigPrefix));
        }

        // 2. Options Registration [BB.06]
        VKAISKOptions options = services.AddVKBlockOptions<VKAISKOptions>(configuration!, transform);

        // 3. Mark-Self (Metadata) [AP.02]
        services.AddVKBlockMarker<VKAISKBlock>();

        // 4. Initialize Builder
        var builder = new AISKBlockBuilder(services, configuration.GetSection(VKBlocksConstants.VKBlocksConfigPrefix));

        // 5. Early Return Check
        if (!options.Enabled)
        {
            return builder;
        }

        // 6. Register Implementations
        services.AddVKAISKImplementations(configuration);

        return builder;
    }
}
