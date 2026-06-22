using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.Common.DependencyInjection.Internal;

/// <summary>
/// Principal registration logic for the AI Vector Store building block.
/// Following BB.03.2 execution sequence.
/// </summary>
internal static class VectorStoreBlockRegistration
{
    internal static IVKVectorStoreBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKVectorStoreOptions, VKVectorStoreOptions>? transform = null)
    {
        VKGuard.NotNull(services);
        VKGuard.NotNull(configuration);

        // 1. [BB.03] Check-Self & Prerequisite
        if (services.IsVKBlockRegistered<VKVectorStoreBlock>())
        {
            return new VectorStoreBlockBuilder(services, configuration);
        }

        // 2. [BB.03] Options Registration (ADR-016)
        VKVectorStoreOptions options = services.AddVKBlockOptions<VKVectorStoreOptions>(configuration, transform);

        // 3. [BB.03] Mark-Self
        services.AddVKBlockMarker<VKVectorStoreBlock>();

        // 4. [BB.03] Validate Options

        var builder = new VectorStoreBlockBuilder(services, configuration);

        // 6. [BB.03] Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        return builder;
    }
}
