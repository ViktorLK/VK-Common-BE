using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace VK.Blocks.VectorSearch.QueryRewrite.Internal;

/// <summary>
/// Query Rewrite feature marker and registration hub.
/// </summary>
internal sealed partial class QueryRewriteFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKQueryRewriteOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKQueryRewriter, NoOpQueryRewriter>();
        services.TryAddScoped<IVKVectorSearchBeforePipelineStage, DefaultQueryRewriteStage>();
    }

    static partial void ValidateCustom(VKQueryRewriteOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
