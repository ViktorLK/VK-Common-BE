using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.VectorSearch.Expansion.Internal;
using VK.Blocks.VectorSearch.QueryRewrite.Internal;
using VK.Blocks.VectorSearch.Retrieval.Internal;
using VK.Blocks.VectorSearch.Fusion.Internal;
using VK.Blocks.VectorSearch.SemanticCache.Internal;
using VK.Blocks.VectorSearch.Pipeline.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.VectorSearch.Common.DependencyInjection.Internal;

/// <summary>
/// Internal central registration logic for AI Recall (BB.03).
/// </summary>
internal static class VectorSearchBlockRegistration
{
    public static IVKVectorSearchBuilder Register(
        IServiceCollection services,
        IConfiguration configuration,
        Func<VKVectorSearchOptions, VKVectorSearchOptions>? transform = null)
    {
        var builder = new VectorSearchBlockBuilder(services, configuration.GetSection(VKBlocksConstants.VKBlocksConfigPrefix));

        if (services.IsVKBlockRegistered<VKVectorSearchBlock>())
        {
            return builder;
        }

        VKVectorSearchOptions options = services.AddVKBlockOptions(configuration, transform);

        services.AddVKBlockMarker<VKVectorSearchBlock>();

        if (!options.Enabled)
        {
            return builder;
        }

        return builder;
    }
}
