using VK.Blocks.AI.Engram.Compression;
using VK.Blocks.AI.Engram.Compression.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Compression feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIEngramBlock), OptionsType = typeof(VKCompressionOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class CompressionFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKCompressionOptions options)
    {
        services.TryAddScoped<IVKCompressionService, DefaultCompressionService>();
        services.TryAddSingleton<IVKSessionCompressionLock, InMemorySessionCompressionLock>();

        // Register compression background queue & hosted service
        services.TryAddSingleton<CompressionJobQueue>();
        services.AddHostedService<DefaultCompressionBackgroundService>();

        // Register all compression strategy concrete implementations
        services.TryAddScoped<NullCompressionStrategy>();
        services.TryAddScoped<LlmSummaryCompressionStrategy>();
        services.TryAddScoped<KeyValueExtractionCompressionStrategy>();
        services.TryAddScoped<HierarchicalSummaryCompressionStrategy>();
        services.TryAddScoped<TopicSegmentationCompressionStrategy>();

        // Register default IVKCompressionStrategy based on options
        services.TryAddScoped<IVKCompressionStrategy>(sp =>
        {
            var optionsSnapshot = sp.GetRequiredService<IOptionsSnapshot<VKCompressionOptions>>().Value;
            return optionsSnapshot.StrategyType switch
            {
                VKCompressionStrategyType.LlmSummary => sp.GetRequiredService<LlmSummaryCompressionStrategy>(),
                VKCompressionStrategyType.KeyValueExtraction => sp.GetRequiredService<KeyValueExtractionCompressionStrategy>(),
                VKCompressionStrategyType.HierarchicalSummary => sp.GetRequiredService<HierarchicalSummaryCompressionStrategy>(),
                VKCompressionStrategyType.TopicSegmentation => sp.GetRequiredService<TopicSegmentationCompressionStrategy>(),
                _ => sp.GetRequiredService<NullCompressionStrategy>()
            };
        });
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKCompressionOptions options, List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (options.SummaryTriggerTokenThreshold <= 0)
        {
            failures.Add("VKCompressionOptions.SummaryTriggerTokenThreshold must be greater than zero.");
        }

        if (options.SummaryTargetTokens <= 0)
        {
            failures.Add("VKCompressionOptions.SummaryTargetTokens must be greater than zero.");
        }
    }
}
