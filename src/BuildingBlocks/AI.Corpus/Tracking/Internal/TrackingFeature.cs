using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche;

namespace VK.Blocks.AI.Corpus.Tracking.Internal;

/// <summary>
/// Hook class for registering Tracking-related DI dependencies and validations.
/// Hooks into the source-generated [VKFeature] system.
/// </summary>
internal sealed partial class TrackingFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKTrackingOptions options)
    {
        _ = options;

        services.TryAddSingleton<IVKKnowledgeInjectionStore, InMemoryKnowledgeInjectionStore>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKPsycheAfterPipelineStage, DefaultKnowledgeInjectionStage>());
    }

    static partial void ValidateCustom(VKTrackingOptions options, List<string> failures)
    {
        _ = options;
    }
}
