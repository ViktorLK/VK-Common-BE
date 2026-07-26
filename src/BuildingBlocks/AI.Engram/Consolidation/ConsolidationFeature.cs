using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Engram.Consolidation;
using VK.Blocks.AI.Engram.Consolidation.Internal;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Consolidation feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIEngramBlock), OptionsType = typeof(VKConsolidationOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class ConsolidationFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKConsolidationOptions options)
    {
        services.TryAddSingleton<IVKMemoryExtractor, DefaultMemoryExtractor>();
        services.TryAddSingleton<IVKContentSanitizer, DefaultContentSanitizer>();
        services.TryAddSingleton<IVKSchemaMerger, DefaultSchemaMerger>();
        services.TryAddSingleton<SimilarityDeduplicator>();
        services.TryAddScoped<IVKConsolidationPersistenceManager, DefaultConsolidationPersistenceManager>();
        services.TryAddScoped<IVKConsolidationService, DefaultConsolidationService>();

        // Register consolidation job queue & background hosted service
        services.TryAddSingleton<ConsolidationJobQueue>();
        services.AddHostedService<DefaultConsolidationBackgroundService>();

        services.TryAddScoped<DefaultConsolidationStage>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheAfterPipelineStage, DefaultConsolidationStage>());
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKConsolidationOptions options, List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (options.SimilarityThreshold is < 0 or > 1)
        {
            failures.Add("VKConsolidationOptions.SimilarityThreshold must be in range [0, 1].");
        }

        if (options.DropLowerThreshold is < 0 or > 1)
        {
            failures.Add("VKConsolidationOptions.DropLowerThreshold must be in range [0, 1].");
        }

        if (options.MaxBatchSize <= 0)
        {
            failures.Add("VKConsolidationOptions.MaxBatchSize must be greater than zero.");
        }

        if (options.MaxMemoryContentLength <= 0)
        {
            failures.Add("VKConsolidationOptions.MaxMemoryContentLength must be greater than zero.");
        }

        if (options.AutomaticConsolidationIntervalMinutes <= 0)
        {
            failures.Add("VKConsolidationOptions.AutomaticConsolidationIntervalMinutes must be greater than zero.");
        }
    }
}
