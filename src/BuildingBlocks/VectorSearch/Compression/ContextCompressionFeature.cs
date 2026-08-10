using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;
using VK.Blocks.VectorSearch.Compression.Internal;

namespace VK.Blocks.VectorSearch;

/// <summary>
/// Context Compression feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKVectorSearchBlock), OptionsType = typeof(VKContextCompressionOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class ContextCompressionFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKContextCompressionOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKContextCompressionStrategy, DefaultContextCompressionStrategy>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKVectorSearchPipelineStage, DefaultContextCompressionStage>());
    }

    static partial void ValidateFeatureCustom(VKContextCompressionOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
