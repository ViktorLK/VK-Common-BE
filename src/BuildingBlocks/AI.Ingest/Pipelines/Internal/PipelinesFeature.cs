using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Ingest.Common.Models.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Ingest.Pipelines.Internal;

/// <summary>
/// Service registration and options validation for Pipelines feature.
/// </summary>
internal sealed partial class PipelinesFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKPipelinesOptions options)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKIngestPipelineStage, DocumentLoadStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKIngestPipelineStage, EmbeddingGenerationStage>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKIngestPipelineStage, DocumentWriteSinkStage>());
        services.TryAddScoped<IngestPipelineExecutor>();
        services.TryAddScoped<IVKIngestPipeline, DefaultIngestPipeline>();
    }

    // [SG Hook]
    static partial void ValidateCustom(VKPipelinesOptions options, System.Collections.Generic.List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
