using VK.Blocks.AI.Corpus.Tracking.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Corpus;

/// <summary>
/// Hook class for registering Tracking-related DI dependencies and validations.
/// Hooks into the source-generated [VKFeature] system.
/// </summary>
[VKFeature(typeof(VKAICorpusBlock), OptionsType = typeof(VKTrackingOptions))]
internal sealed partial class TrackingFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKTrackingOptions options)
    {
        _ = options;

        services.TryAddSingleton<IVKKnowledgeInjectionStore, InMemoryKnowledgeInjectionStore>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsychePipelineStage, DefaultKnowledgeInjectionStage>());
    }

    static partial void ValidateFeatureCustom(VKTrackingOptions options, List<string> failures)
    {
        _ = options;
    }
}
