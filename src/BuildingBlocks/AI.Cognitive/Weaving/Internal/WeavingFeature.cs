using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// // [AP.03] Internal implementation inside Internal/ folder without VK prefix
namespace VK.Blocks.AI.Cognitive.Weaving.Internal;

internal static class WeavingFeature
{
    public static void Register(IVKAICognitiveBuilder builder, System.Action<VKWeavingOptions>? configure = null)
    {
        var services = builder.Services;

        if (configure != null)
        {
            services.Configure(configure);
        }

        // Register default extractors using non-generic IVKPromptExtractor
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPromptExtractor, ChatMessageExtractor>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPromptExtractor, PipelineSystemInstructionsExtractor>());

        // Register pipeline stages
        services.TryAddScoped<IVKPromptExtractionCoordinator, BasicPromptExtractionCoordinator>();
        services.TryAddScoped<IVKPromptScorer, BasicPromptScorer>();
        services.TryAddScoped<IVKPromptPruner, BasicPromptPruner>();
        services.TryAddScoped<IVKBudgetTruncator, BasicBudgetTruncator>();
        services.TryAddScoped<IVKPromptFormatter<VKDefaultModelMarker>, BasicPromptFormatter>();
        services.TryAddScoped<IVKTapestryWeaver, BasicTapestryWeaver>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKOrchestrationPipelineStage, DefaultWeavingPipelineStage>());

        // Register orchestration engine
        services.TryAddScoped<IVKPromptWeavingEngine, DefaultPromptWeavingEngine>();
    }
}
