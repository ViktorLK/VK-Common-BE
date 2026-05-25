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

        // Register default extractors
        services.TryAddScoped<IVKPromptExtractor<IEnumerable<VKKnowledgeEntry>>, KnowledgeEntryExtractor>();
        services.TryAddScoped<IVKPromptExtractor<IEnumerable<VKChatMessage>>, ChatMessageExtractor>();

        // Register pipeline stages
        services.TryAddScoped<IVKPromptScorer, BasicPromptScorer>();
        services.TryAddScoped<IVKPromptPruner, BasicPromptPruner>();
        services.TryAddScoped<IVKBudgetTruncator, BasicBudgetTruncator>();
        services.TryAddScoped<IVKPromptFormatter<VKDefaultModelMarker>, BasicPromptFormatter>();
        services.TryAddScoped<IVKTapestryWeaver, BasicTapestryWeaver>();

        // Register orchestration engine
        services.TryAddScoped<IVKPromptWeavingEngine, DefaultPromptWeavingEngine>();
    }
}
