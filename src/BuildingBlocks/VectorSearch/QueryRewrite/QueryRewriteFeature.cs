using VK.Blocks.VectorSearch.QueryRewrite.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
namespace VK.Blocks.VectorSearch;

/// <summary>
/// Query Rewrite feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock), OptionsType = typeof(VKQueryRewriteOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class QueryRewriteFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKQueryRewriteOptions options)
    {
        _ = options;
        services.TryAddSingleton<IVKQueryRewriter, NoOpQueryRewriter>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKVectorSearchPipelineStage, DefaultQueryRewriteStage>());
    }

    static partial void ValidateFeatureCustom(VKQueryRewriteOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
