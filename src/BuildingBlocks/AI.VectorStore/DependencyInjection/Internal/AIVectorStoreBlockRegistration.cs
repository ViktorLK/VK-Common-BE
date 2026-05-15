using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the AI Vector Store building block.
/// Following BB.03.2 execution sequence.
/// </summary>
internal static class AIVectorStoreBlockRegistration
{
    internal static IVKAIVectorStoreBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKAIVectorStoreOptions, VKAIVectorStoreOptions>? transform = null)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);

        // 1. [BB.03] Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKAIVectorStoreBlock>())
        {
            return new AIVectorStoreBlockBuilder(services, configuration);
        }

        // 2. [BB.03] Options Registration (ADR-016)
        VKAIVectorStoreOptions options = services.AddVKBlockOptions<VKAIVectorStoreOptions>(configuration, transform);

        // 3. [BB.03] Mark-Self
        services.AddVKBlockMarker<VKAIVectorStoreBlock>();

        // 4. [BB.03] Validate Options
        services.TryAddEnumerableSingleton<IValidateOptions<VKAIVectorStoreOptions>, AIVectorStoreOptionsValidator>();

        var builder = new AIVectorStoreBlockBuilder(services, configuration);

        // 6. [BB.03] Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        return builder;
    }
}
