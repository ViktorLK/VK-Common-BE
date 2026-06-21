using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest.Common.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the AI Ingest building block.
/// Following BB.03.2 execution sequence.
/// </summary>
internal static class AIIngestBlockRegistration
{
    internal static IVKAIIngestBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAIIngestOptions, VKAIIngestOptions>? transform = null)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);

        // 1. [BB.03] Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKAIIngestBlock>())
        {
            return new AIIngestBlockBuilder(services, configuration);
        }

        // 2. [BB.03] Options Registration
        VKAIIngestOptions options = services.AddVKBlockOptions<VKAIIngestOptions>(configuration, transform);

        // 3. [BB.03] Mark-Self
        services.AddVKBlockMarker<VKAIIngestBlock>();

        // 4. [BB.03] Validate Options
        services.TryAddEnumerableSingleton<IValidateOptions<VKAIIngestOptions>, AIIngestOptionsValidator>();

        var builder = new AIIngestBlockBuilder(services, configuration);

        // 8. [BB.03] Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        return builder;
    }
}
