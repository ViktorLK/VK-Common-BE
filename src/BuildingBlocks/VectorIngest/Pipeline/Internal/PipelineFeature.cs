using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.VectorIngest.Common.Models.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest.Pipeline.Internal;

/// <summary>
/// Service registration and options validation for Pipeline feature.
/// </summary>
internal sealed partial class PipelineFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKPipelineOptions options)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKIngestPipelineStage, DocumentLoadStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKIngestPipelineStage, DocumentWriteSinkStage>());
        services.TryAddScoped<IngestPipelineExecutor>();
        services.TryAddScoped<IVKIngestPipeline, DefaultIngestPipeline>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKPipelineOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
