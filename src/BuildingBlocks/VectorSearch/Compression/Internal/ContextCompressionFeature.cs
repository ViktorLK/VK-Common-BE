using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.VectorSearch;
using VK.Blocks.VectorSearch.Compression.Internal;

namespace VK.Blocks.VectorSearch.ContextCompression.Internal;

/// <summary>
/// Context Compression feature marker and registration hub.
/// </summary>
internal sealed partial class ContextCompressionFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKContextCompressionOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKContextCompressionStrategy, DefaultContextCompressionStrategy>();
        services.TryAddScoped<IVKVectorSearchAfterPipelineStage, DefaultContextCompressionStage>();
    }

    static partial void ValidateCustom(VKContextCompressionOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
