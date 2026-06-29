using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Common.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the AI Ingest building block.
/// Following BB.03.2 execution sequence.
/// </summary>
internal static class AIIngestBlockRegistration
{
    internal static IVKVectorIngestBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKVectorIngestOptions, VKVectorIngestOptions>? transform = null)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);

        // 1. [BB.03] Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKVectorIngestBlock>())
        {
            return new AIIngestBlockBuilder(services, configuration);
        }

        // 2. [BB.03] Options Registration
        VKVectorIngestOptions options = services.AddVKBlockOptions<VKVectorIngestOptions>(configuration, transform);

        // 3. [BB.03] Mark-Self
        services.AddVKBlockMarker<VKVectorIngestBlock>();

        // 4. [BB.03] Validate Options
        var builder = new AIIngestBlockBuilder(services, configuration);

        // 8. [BB.03] Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        return builder;
    }
}
